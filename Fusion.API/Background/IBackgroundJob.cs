

namespace Fusion.Service.Commons.Background;

public interface IBackgroundJob
{
    string Name { get; }              // tên job để log cho dễ debug
    TimeSpan Interval { get; }        // chạy mỗi bao lâu
    Task ExecuteAsync(IServiceProvider services, CancellationToken ct);
}
