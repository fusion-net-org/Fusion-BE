
using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.SubscriptionPlan.Responses;

public class PlanFeatureChipResponse
{
    public string Name { get; set; } = "";
}

public class PlanPricePreviewResponse
{
    public decimal Amount { get; set; }
    public decimal? NewAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string Currency { get; set; } = "VND";
    public BillingPeriod BillingPeriod { get; set; }  // Month/Year...
    public int PeriodCount { get; set; }              // 1/3/12...
    public ChargeUnit ChargeUnit { get; set; }        // PerSubscription/PerSeat
    public PaymentMode PaymentMode { get; set; }      // Prepaid/Installments
    public int? InstallmentCount { get; set; }
    public BillingPeriod? InstallmentInterval { get; set; }
}

public class SubscriptionPlanCustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public bool IsFullPackage { get; set; }
    public LicenseScope LicenseScope { get; set; }
    public int? CompanyShareLimit { get; set; }
    public int? SeatsPerCompanyLimit { get; set; }

    public PlanPricePreviewResponse? Price { get; set; }
    public List<PlanFeatureChipResponse> FeaturesPreview { get; set; } = new(); 
}
