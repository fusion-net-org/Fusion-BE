using Fusion.Repository.Bases.Page;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyFriendshipRepository
    {
        Task<CompanyFriendship> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId);
        Task<CompanyFriendship> CancelCompanyFriendship(long id);
        Task<CompanyFriendship> AcceptCompanyFriendship(long id);
        Task<List<CompanyFriendship>> GetCompanyFriendshipByStatus(string status);
        Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID, PagedRequest request, CancellationToken cancellationToken = default);
        Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId);
    }
}
