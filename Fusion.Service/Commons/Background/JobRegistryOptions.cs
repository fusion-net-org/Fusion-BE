

namespace Fusion.Service.Commons.Background;


public sealed class JobRegistration
{
    public required string Key { get; init; }
    public required Type JobType { get; init; }  
    public required JobSchedule Schedule { get; init; }
    public bool Enabled { get; init; } = true;
    public bool SingleInstance { get; init; } = true;
}

public sealed class JobRegistryOptions
{
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public List<JobRegistration> Jobs { get; } = new();
}