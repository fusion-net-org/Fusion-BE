

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<PagedResult<User>> GetPagedAdminUsersAsync(AdminUserPagedRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<User>> GetPagedCompanyUsersAsync(CompanyUserPagedRequest request, CancellationToken cancellationToken = default);
        Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserByGoogleSubAsync(string googleSub, CancellationToken cancellationToken = default);
        Task<bool> CheckEmailExistAsync(string email, CancellationToken cancellationToken = default);
        Task<PagedResult<User>> GetAllUsersAsync(PagedRequest request,CancellationToken cancellationToken = default);
        Task<User?> GetOwnerUserByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    }
}
