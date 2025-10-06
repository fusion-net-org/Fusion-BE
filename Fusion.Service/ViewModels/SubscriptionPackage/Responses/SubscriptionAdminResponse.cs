
namespace Fusion.Service.ViewModels.SubscriptionPackage.Responses;

public record SubscriptionAdminResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public decimal Price { get; init; }
    public int QuotaCompany { get; init; }
    public int QuotaProject { get; init; }
    public string Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
