

namespace Fusion.Service.ViewModels.SubscriptionPackage.Responses;

public record SubscriptionResponse
{
    public Guid Id { get; set; }
    public string Name { get; init; }
    public decimal Price { get; init; }
    public int QuotaCompany { get; init; }
    public int QuotaProject { get; init; }
    public string Description { get; init; } 
}
