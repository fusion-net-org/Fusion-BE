

namespace Fusion.Service.Commons.Background;


public interface IBackgroundJob
{
    string Key { get; }
    Task RunAsync(IServiceProvider services, CancellationToken ct);
}
