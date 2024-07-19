using System.Diagnostics.CodeAnalysis;

namespace component;

public class Worker(ILogger<Worker> logger, ISearchGenerationService service, int interval = Worker.ReportingInterval) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly ISearchGenerationService _searchGenerationService = service;
    private readonly int _reportInterval = interval;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateLogAsync(stoppingToken);
        }
    }
    [ExcludeFromCodeCoverage(Justification = "Private member fulled tested from public accessor.")]
    private async Task UpdateLogAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(_reportInterval, stoppingToken);
            _searchGenerationService.Report();
        }
        catch (Exception)
        {
            // no action taken on exceptions
        }
    }

    private const int ReportingInterval = 600000; // 10 minutes
}
