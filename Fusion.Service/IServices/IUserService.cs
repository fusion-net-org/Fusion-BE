
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.IServices;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<CompanyUserResponse>> GetPagedCompanyUsersAsync(
            CompanyUserPagedRequest request,
            CancellationToken cancellationToken = default);
    Task<PagedResult<AdminUserResponse>> GetPagedAdminUsersAsync(
        AdminUserPagedRequest request,
        CancellationToken cancellationToken = default);

    Task<SelfUserResponse?> GetSelfUserAsync(CancellationToken cancellationToken = default);
    Task<SelfUserResponse?> UpdateSelfUserAsync(UpdateSelfUserRequest request, CancellationToken cancellationToken = default);

}
