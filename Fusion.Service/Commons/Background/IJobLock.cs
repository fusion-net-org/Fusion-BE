

namespace Fusion.Service.Commons.Background;

public interface IJobLock
{
    Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, TimeSpan ttl, CancellationToken ct);
}
