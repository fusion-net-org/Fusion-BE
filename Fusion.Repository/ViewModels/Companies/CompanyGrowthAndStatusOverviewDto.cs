

namespace Fusion.Repository.ViewModels.Companies;


public sealed class CompanyGrowthPointDto
{
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
    public int NewCompanies { get; set; }
    public int CumulativeCompanies { get; set; }
}

public sealed class CompanyGrowthAndStatusOverviewDto
{
    public int TotalCompanies { get; set; }
    public int ActiveCompanies { get; set; }
    public int DeletedCompanies { get; set; }
    public int NewCompaniesLast30Days { get; set; }

    public List<CompanyGrowthPointDto> Growth { get; set; } = new();
}
