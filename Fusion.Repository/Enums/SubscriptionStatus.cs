

namespace Fusion.Repository.Enums;

public enum SubscriptionStatus
{
    Active,
    Inactive,
    Refunded
}
public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded,
    Cancelled
}

public enum BillingPriod
{
    Week,
    Month,
    Year
}

public enum FeatureKeys
{
    Project,
    Company, 
    Sprint,
    Partner
}