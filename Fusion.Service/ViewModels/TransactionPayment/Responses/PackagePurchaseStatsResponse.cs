

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class PackagePurchaseStatsItem
{
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;

    public int Orders { get; set; }             
    public decimal Revenue { get; set; }       

    public double OrderShare { get; set; }     
    public double RevenueShare { get; set; }    
}

public class PackagePurchaseStatsResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<PackagePurchaseStatsItem> Items { get; set; } = new();
}
