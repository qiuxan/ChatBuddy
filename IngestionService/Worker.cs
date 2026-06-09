using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.Extensions.VectorData;
using Microsoft.ML.Tokenizers;

namespace IngestionService;

public class Worker(
    ILoggerFactory loggerFactory,
    ILogger<Worker> logger,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore
    ) : BackgroundService
{
    const string trackingFilePath = "tracking.txt";
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var directoryInfo = new DirectoryInfo("incident_dataset");
        logger.LogInformation("Working directory: {dir}", Directory.GetCurrentDirectory());
        logger.LogInformation("incident_dataset exists: {exists}", directoryInfo.Exists);

        await File.Create(trackingFilePath).DisposeAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedFiles = (await File.ReadAllLinesAsync(trackingFilePath, stoppingToken)).ToHashSet();

                var filesProcess = directoryInfo.EnumerateFiles("*.md")
                    .Where(file => !processedFiles.Contains(file.FullName))
                    .ToList();

                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                logger.LogInformation("Files to process: {files}", string.Join(',', filesProcess.Select(x => x.Name)));

                if (filesProcess.Count == 0)
                {
                    logger.LogInformation("No new files to process.");
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                using var vectorStoreWriter = new VectorStoreWriter<string>(vectorStore, 384, new VectorStoreWriterOptions
                {
                    CollectionName = "data-icm-chunks",
                    DistanceFunction = DistanceFunction.CosineDistance,
                    IncrementalIngestion = false
                });

                var pipeline = new IngestionPipeline<string>(
                    reader: new IncReader(),
                    chunker: new SemanticSimilarityChunker(embeddingGenerator, new IngestionChunkerOptions(TiktokenTokenizer.CreateForModel("gpt-4o"))),
                    writer: vectorStoreWriter,
                    loggerFactory: loggerFactory
                );

                await foreach (var result in pipeline.ProcessAsync(filesProcess, stoppingToken))
                {
                    if (result.Succeeded)
                        logger.LogInformation("Processed: {fileId}", result.DocumentId);
                    else
                        logger.LogError("Failed to process: {filesId}", result.DocumentId);
                }

                await File.AppendAllLinesAsync(trackingFilePath, filesProcess.Select(f => f.FullName), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during ingestion pipeline execution");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    public class IncReader : IngestionDocumentReader
    {
        readonly MarkdownReader _markdownReader = new();
        public override Task<IngestionDocument> ReadAsync(Stream source, string identifier, string mediaType,
            CancellationToken cancellationToken = new CancellationToken())
        {
            Debug.WriteLine(identifier);
            Debug.WriteLine(mediaType);
            return _markdownReader.ReadAsync(source,identifier,mediaType,cancellationToken);
        }
    }
}