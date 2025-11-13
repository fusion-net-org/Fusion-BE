

//using Fusion.Repository.Enums;
//using System.ComponentModel.DataAnnotations;

//namespace Fusion.Service.ViewModels.CompanySubscription.Requests
//{
//    public class CompanySubscriptionCreateRequest
//    {
//        [Required]
//        public Guid CompanyId { get; set; }

//        [Required]
//        public Guid UserSubscriptionId { get; set; }

//        [Required]
//        public List<CompanySubscriptionEntitlementCreateRequest> Entitlements { get; set; } = new();
//    }
//    public class CompanySubscriptionEntitlementCreateRequest
//    {
//        [Required]
//        public FeatureKeys FeatureKey { get; set; }

//        [Required]
//        [Range(1, int.MaxValue)]
//        public int Quantity { get; set; }
//    }

//}
