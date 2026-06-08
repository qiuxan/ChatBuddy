namespace IngestionService;

public class Worker(ILogger<Worker> logger) : BackgroundService
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
            
            await File.AppendAllLinesAsync(trackingFilePath, filesProcess.Select(_ => _.FullName), stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}