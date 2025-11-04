using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Fusion.Service.Commons.Background;

public sealed class SchedulerHostedService : BackgroundService
{
    private readonly ILogger<SchedulerHostedService> _logger;
    private readonly IServiceProvider _services;
    private readonly JobRegistryOptions _registry;
    private readonly IJobLock _lock;
    private readonly TimeZoneInfo _tz;

    public SchedulerHostedService(
        ILogger<SchedulerHostedService> logger,
        IServiceProvider services,
        IOptions<JobRegistryOptions> registry,
        IJobLock @lock)
    {
        _logger = logger;
        _services = services;
        _registry = registry.Value;
        _lock = @lock;
        _tz = ResolveTz(_registry.TimeZone);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _registry.Jobs.Where(j => j.Enabled).ToList();
        _logger.LogInformation("Scheduler started. Enabled jobs: {Count}", enabled.Count);
        if (enabled.Count == 0) _logger.LogWarning("No jobs registered.");
        var tasks = enabled.Select(j => RunLoopAsync(j, stoppingToken));
        return Task.WhenAll(tasks);
    }

    private async Task RunLoopAsync(JobRegistration reg, CancellationToken ct)
    {
        _logger.LogInformation("Job {Key} registered. SingleInstance={Single}", reg.Key, reg.SingleInstance);

        if (reg.Schedule.RunOnStartup) await SafeRunOnce(reg, ct);

        while (!ct.IsCancellationRequested)
        {
            var next = reg.Schedule.GetNextRun(DateTimeOffset.UtcNow, _tz);
            var delay = next - DateTimeOffset.UtcNow;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            try { await Task.Delay(delay, ct); } catch (TaskCanceledException) { break; }
            await SafeRunOnce(reg, ct);
        }
    }

    private async Task SafeRunOnce(JobRegistration reg, CancellationToken ct)
    {
        var lockTtl = TimeSpan.FromMinutes(15);

        IAsyncDisposable? handle = null;
        if (reg.SingleInstance)
        {
            handle = await _lock.TryAcquireAsync($"job:{reg.Key}", lockTtl, ct);
            if (handle is null)
            {
                _logger.LogDebug("Skip {Key} — locked by another instance.", reg.Key);
                return;
            }
        }

        await using (handle)
        {
            using var scope = _services.CreateScope();
            var job = (IBackgroundJob)scope.ServiceProvider.GetRequiredService(reg.JobType);

            var sw = Stopwatch.StartNew();
            try
            {
                await job.RunAsync(scope.ServiceProvider, ct);
                sw.Stop();
                _logger.LogInformation("Job {Key} finished in {Elapsed} ms", reg.Key, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Job {Key} failed after {Elapsed} ms", reg.Key, sw.ElapsedMilliseconds);
            }
        }
    }

    private static TimeZoneInfo ResolveTz(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } 
    }
}