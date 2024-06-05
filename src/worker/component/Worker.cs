namespace component;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISearchGenerationService _searchGenerationService;
    public Worker(ILogger<Worker> logger, ISearchGenerationService service)
    {
        _logger = logger;
        _searchGenerationService = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(ReportingInterval, stoppingToken);
            _searchGenerationService.Report();
        }
    }

    private const int ReportingInterval = 600000; // 10 minutes
}
