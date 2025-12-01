


using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.UserSubscription.Responses;

public class UserSubscriptionDetailResponse : UserSubscriptionResponse
{
    public bool IsFullPackage { get; set; }
    public string LicenseScope { get; set; } = "";
    public int? CompanyShareLimit { get; set; }
    public int? SeatsPerCompanyLimit { get; set; }

    public int PeriodCount { get; set; }
    public string BillingPeriod { get; set; } = "";
    public string PaymentMode { get; set; } = "";
    public int? InstallmentCount { get; set; }
    public string? InstallmentInterval { get; set; }

    public List<EntitlementVm> Entitlements { get; set; } = new();
}
public class EntitlementVm
{
    public Guid Id { get; set; }              // Id của entitlement
    public Guid FeatureId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool Enabled { get; set; }

    public int? MonthlyLimit { get; set; }    // limit snapshot theo UserSubscriptionEntitlement
    public int? LimitUnit { get; set; }       
}