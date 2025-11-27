
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.Users;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.IServices;

public interface IUserService
{
    Task<List<RoleDto>> GetRolesByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<SelfUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<CompanyUserResponse>> GetPagedCompanyUsersAsync(
            CompanyUserPagedRequest request,
            CancellationToken cancellationToken = default);
    Task<PagedResult<AdminUserResponse>> GetPagedAdminUsersAsync(
        AdminUserPagedRequest request,
        CancellationToken cancellationToken = default);

    Task<SelfUserResponse?> GetSelfUserAsync(CancellationToken cancellationToken = default);
    Task<SelfUserResponse?> UpdateSelfUserAsync(UpdateSelfUserRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<SelfUserResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<SelfUserResponse?> GetOwnerUserByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<SelfUserResponse?> UpdateSelfUserByAdminAsync(Guid id, UpdateSelfUserRequest request, CancellationToken cancellationToken);
    Task<SelfUserResponse?> UpdateStatus(Guid id, bool Status, CancellationToken cancellationToken);
    Task<User?> GetFullInfoByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // ================ Overview Methods ===================
    Task<UserStatusResponse> GetCountUserByStatusAsync(CancellationToken cancellationToken = default);
    Task<UserGrowthAndStatusOverviewResponse> GetUserGrowthAndStatusOverviewAsync(int months = 12, CancellationToken cancellationToken = default);
    Task<List<UserCompanyDistributionPoint>> GetTopCompaniesByUserCountAsync( int top = 10,CancellationToken cancellationToken = default);
    Task<UserPermissionLevelOverviewResponse> GetUserPermissionLevelOverviewAsync(CancellationToken cancellationToken = default);
    Task<AnalyticsUserResponse> GetAnalyticsUserAsync(Guid userId,
 CancellationToken token);
}
