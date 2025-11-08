

using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.CompanySubscription.Requests
{
    public class CompanySubscriptionUpdateRequest
    {
        [Required]
        public Guid Id { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public List<CompanySubscriptionEntitlementUpdateDto>? Entitlements { get; set; }

        public List<CompanySubscriptionRoleUpdateDto>? Roles { get; set; }
    }

    public class CompanySubscriptionEntitlementUpdateDto
    {
        public Guid? Id { get; set; }
        public int? Quantity { get; set; }
    }

    public class CompanySubscriptionRoleUpdateDto
    {
        public Guid? Id { get; set; }
        public string? NameRole { get; set; }
    }
}
