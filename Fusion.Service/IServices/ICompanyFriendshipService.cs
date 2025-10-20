using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Partner;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.IServices
{
    public interface ICompanyFriendshipService
    {
        Task<CompanyFriendshipResponse> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId, string? note);
        Task<CompanyFriendshipResponse> CancelCompanyFriendship(long id, Guid currentUserId);
        Task<CompanyFriendshipResponse> AcceptCompanyFriendship(long id, Guid currentUserId);
        Task<PagedResult<CompanyFriendshipResponse>> GetCompanyFriendshipByStatus(Guid ownerUserID, string status, PagedRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<CompanyFriendshipResponse>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID, CompanyFriendshipSearchRequest request, CancellationToken cancellationToken = default);
        Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId);

        Task<List<CompanyFriendshipResponse>> GetCompanyFriendshipByCompanyID(Guid userID, Guid companyID);

        /*************************************************************Mobile**************************************************************************/

        Task<PagedResult<PartnerResponse>> GetCompanyFriendshipByCompanyID(Guid ownerUserID, Guid companyID, CompanyFriendshipSearchRequest request, CancellationToken token);

        Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId, Guid companyId);

    }
}
