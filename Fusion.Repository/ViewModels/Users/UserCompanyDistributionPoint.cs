

namespace Fusion.Repository.ViewModels.Users;

public class UserCompanyDistributionPoint
{
    public Guid? CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}
