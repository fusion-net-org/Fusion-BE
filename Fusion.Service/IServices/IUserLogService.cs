
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Entities;

namespace Fusion.Service.IServices;

public interface IUserLogService
{
    Task<UserLog> CreateLog(UserLog log, CancellationToken cancellationToken = default);
    public Task<PagedResult<UserLog>> GetUserLogByIdAsync(
    Guid actorUserId,
    UserLogSearchRequest request,
    CancellationToken cancellationToken = default);
    public Task<PagedResult<UserLog>> GetAllUserLogAsync(
      UserLogSearchRequest request,
      CancellationToken cancellationToken = default);
}
