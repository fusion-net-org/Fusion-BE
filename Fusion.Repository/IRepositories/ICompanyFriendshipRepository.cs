using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Partner;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyFriendshipRepository
    {
        Task<CompanyFriendship> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId, string? note);
        Task<CompanyFriendship> CancelCompanyFriendship(long id, Guid currentUserId);
        Task<CompanyFriendship> AcceptCompanyFriendship(long id, Guid currentUserId);
        Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByStatus(Guid ownerUserID,string status, PagedRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID, CompanyFriendshipSearchRequest request, CancellationToken cancellationToken = default);
        Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId);
        Task<List<CompanyFriendship>> GetCompanyFriendshipByCompanyID(Guid userID, Guid companyID);


    }
}
