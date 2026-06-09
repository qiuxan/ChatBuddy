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
        await File.Create(trackingFilePath).DisposeAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            var processedFiles = (await File.ReadAllLinesAsync(trackingFilePath, stoppingToken)).ToHashSet();

            var filesProcess = directoryInfo.EnumerateFiles("*.md")
                .Where(file => !processedFiles.Contains(file.FullName) );
            
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                logger.LogInformation("Files to process: {files}", string.Join(',', filesProcess.Select(x => x.Name)));
            }

            using var vectorStoreWriter = new VectorStoreWriter<string>(vectorStore,384, new VectorStoreWriterOptions
            {
                CollectionName ="data-icm-chunks",
                DistanceFunction = DistanceFunction.CosineDistance,
                IncrementalIngestion = true
                
            });
            var pipeline = new IngestionPipeline<string>(
                reader:new IncReader(),
                chunker:new SemanticSimilarityChunker(embeddingGenerator, new IngestionChunkerOptions(TiktokenTokenizer.CreateForModel("gpt-4o"))),
                writer:vectorStoreWriter,
                loggerFactory: loggerFactory
            );

            await foreach (var result in pipeline.ProcessAsync(filesProcess, stoppingToken))
            {
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to process: {filesId} ",result.DocumentId);
                }
            }

            await File.AppendAllLinesAsync(trackingFilePath, filesProcess.Select(_ => _.FullName), stoppingToken);

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