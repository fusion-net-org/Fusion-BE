

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class CompanyFriendshipRequest
    {
        public Guid CompanyAId { get; set; }
        public Guid CompanyBId { get; set; }
        public Guid RequesterId { get; set; }
    }
}
