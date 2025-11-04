using Fusion.Service.Commons.Background;
using Fusion.Service.IServices;
using Microsoft.Extensions.Logging;

namespace Fusion.Service.Jobs;

public sealed class CheckUserSubscriptionJob : IBackgroundJob
{
    public string Key => "subscription.check";
    private readonly IUserSubscriptionService _svc;
    private readonly ILogger<CheckUserSubscriptionJob> _log;

    public CheckUserSubscriptionJob(IUserSubscriptionService svc, ILogger<CheckUserSubscriptionJob> log)
    {
        _svc = svc; _log = log;
    }

    public async Task RunAsync(IServiceProvider services, CancellationToken ct)
    {
        var affected = await _svc.DeactiveExpiredOrDepleteAsync(ct);
        if(affected > 0)
            _log.LogInformation("Deactivated {Count} expired/depleted subscriptions.", affected);
    }
}
