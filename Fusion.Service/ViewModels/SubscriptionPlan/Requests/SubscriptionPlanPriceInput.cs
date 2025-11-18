using Fusion.Repository.Enums;
using System.ComponentModel.DataAnnotations;


namespace Fusion.Service.ViewModels.SubscriptionPlan.Requests;

public class SubscriptionPlanPriceInput
{
    public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Month;
    public int PeriodCount { get; set; } = 1;

    public ChargeUnit ChargeUnit { get; set; } = ChargeUnit.PerSubscription;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "VND";

    public PaymentMode PaymentMode { get; set; } = PaymentMode.Prepaid;

    // Trả góp (chỉ dùng khi PaymentMode = Installments)
    public BillingPeriod? InstallmentInterval { get; set; }
    public int? InstallmentCount { get; set; }
}
