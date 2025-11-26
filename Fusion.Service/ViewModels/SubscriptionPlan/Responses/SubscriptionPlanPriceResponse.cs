

using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses
{
    public class SubscriptionPlanPriceResponse
    {
        public Guid Id { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public int PeriodCount { get; set; }
        public ChargeUnit ChargeUnit { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentMode PaymentMode { get; set; }
        public int? InstallmentCount { get; set; }
        public BillingPeriod? InstallmentInterval { get; set; }
        public List<SubscriptionPlanPriceDiscountResponse>? Discounts { get; set; }
    }
}
