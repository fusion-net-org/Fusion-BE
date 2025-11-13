
//using Fusion.Repository.Entities;
//using Fusion.Repository.Enums;

//namespace Fusion.Repository.Bases.Page.UserSubscriptions;

//public class UserSubscriptionPagedRequest : PagedRequest
//{
//    public SubscriptionStatus? status { get; set; }
//    public string? Keyword { get; set; }

//    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
//    {
//        ["planName"] = nameof(UserSubscription.NamePlan),
//        ["status"] = nameof(UserSubscription.Status),
//        ["createdAt"] = nameof(UserSubscription.CreatAt),
//        ["expiredAt"] = nameof(UserSubscription.ExpiredAt)
//    };
//}
