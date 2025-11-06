

using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

    public class SubscriptionPlanDetailResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public SubscriptionPlanPriceResponse? Price { get; set; }            // 1–1
        public List<SubscriptionPlanFeatureResponse> Features { get; set; }  // 1–N
            = new();
    }

    public class SubscriptionPlanPriceResponse
    {
        public BillingPriod BillingPeriod { get; set; }
        public int PeriodCount { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "VND";
        public int RefundWindowDays { get; set; }
        public decimal RefundFeePercent { get; set; }
    }

    public class SubscriptionPlanFeatureResponse
    {
        public string FeatureKey { get; set; } = string.Empty; 
        public int LimitValue { get; set; }
    }