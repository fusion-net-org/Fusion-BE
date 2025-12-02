using Fusion.Service.Commons.Background;
using Fusion.Service.IServices;

namespace Fusion.API.Background.Jobs
{
    public sealed class SubscriptionStatusMonitorJob : IBackgroundJob
    {
        public string Name => "SubscriptionStatusMonitorJob";
        public TimeSpan Interval => TimeSpan.FromSeconds(15);
        public async Task ExecuteAsync(IServiceProvider services, CancellationToken ct)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<SubscriptionStatusMonitorJob>>();

            var userSubService = scope.ServiceProvider
                .GetRequiredService<IUserSubscriptionService>();

            var now = DateTimeOffset.UtcNow;

            logger.LogInformation("[{Job}] Tick at {Now}", Name, now);

            try
            {
                var changed = await userSubService.SyncSubscriptionStatusesByTimeAsync(ct);

                logger.LogInformation(
                    "[{Job}] Executed at {Now}, updated {Count} subscription/company statuses.",
                    Name, now, changed);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "[{Job}] Failed at {Now}.",
                    Name, now);
            }
        }
    }
}