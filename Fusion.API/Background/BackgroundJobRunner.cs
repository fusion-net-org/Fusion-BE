using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Fusion.Service.Commons.Background;

public sealed class BackgroundJobRunner : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IEnumerable<IBackgroundJob> _jobs;
    private readonly ILogger<BackgroundJobRunner> _logger;
    public BackgroundJobRunner(
     IServiceProvider services,
     IEnumerable<IBackgroundJob> jobs,
     ILogger<BackgroundJobRunner> logger)
    {
        _services = services;
        _jobs = jobs;
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Mỗi job chạy trên 1 vòng lặp riêng
        foreach (var job in _jobs)
        {
            _ = RunJobLoopAsync(job, stoppingToken);
        }

        return Task.CompletedTask;
    }

    private async Task RunJobLoopAsync(IBackgroundJob job, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job {JobName} started with interval {Interval}.",
            job.Name, job.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await job.ExecuteAsync(_services, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing job {JobName}.", job.Name);
            }

            try
            {
                await Task.Delay(job.Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // app shutdown, break loop
                break;
            }
        }

        _logger.LogInformation("Job {JobName} stopping.", job.Name);
    }
}
