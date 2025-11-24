
namespace Fusion.Repository.ViewModels.Companies;

public sealed class CompanyProjectLoadBucketDto
{
    public string BucketKey { get; set; } = default!;   // "0", "1-2", "3-5", ...
    public string Label { get; set; } = default!;       // "0 projects", "1–2 projects", ...
    public int CompanyCount { get; set; }               // số company trong bucket
    public int TotalProjects { get; set; }              // tổng projects của bucket
}

public sealed class CompanyProjectLoadOverviewDto
{
    public int TotalCompanies { get; set; }
    public int TotalProjects { get; set; }
    public List<CompanyProjectLoadBucketDto> Buckets { get; set; } = new();
}