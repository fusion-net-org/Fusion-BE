using Fusion.Repository.ViewModels;

namespace Fusion.Service.ViewModels.Admin.Responses
{
    public class PlatformYearOverviewResponse
    {
        public int Year { get; set; }

        public int TotalNewUsers { get; set; }
        public int TotalNewCompanies { get; set; }
        public decimal TotalTransactionAmount { get; set; }

        public List<PlatformMonthlyPointResponse> Months { get; set; } = new();
    }
}
