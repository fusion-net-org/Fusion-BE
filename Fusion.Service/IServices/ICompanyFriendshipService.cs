using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Requests;

namespace Fusion.Service.IServices
{
    public interface ICompanyFriendshipService
    {
        Task<CompanyFriendshipResponse> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId);
        Task<CompanyFriendshipResponse> CancelCompanyFriendship(long id);
        Task<CompanyFriendshipResponse> AcceptCompanyFriendship(long id);
        Task<List<CompanyFriendship>> GetCompanyFriendshipByStatus(string status);
        Task<List<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID);

    }
}
