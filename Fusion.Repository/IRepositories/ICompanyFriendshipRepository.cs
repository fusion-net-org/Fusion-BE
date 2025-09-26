using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyFriendshipRepository
    {
        Task<CompanyFriendship> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId);
        Task<CompanyFriendship> CancelCompanyFriendship(long id);
        Task<CompanyFriendship> AcceptCompanyFriendship(long id);
        Task<List<CompanyFriendship>> GetCompanyFriendshipByStatus(string status);
        Task<List<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID);
    }
}
