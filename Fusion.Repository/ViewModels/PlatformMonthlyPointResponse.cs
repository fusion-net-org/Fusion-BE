
namespace Fusion.Repository.ViewModels
{
    public class PlatformMonthlyPointResponse
    {
        public int Month { get; set; }                 // 1..12
        public int NewUsers { get; set; }              // số user tạo mới trong tháng
        public int NewCompanies { get; set; }          // số company tạo mới trong tháng
        public decimal TotalTransactionAmount { get; set; }
    }
}
