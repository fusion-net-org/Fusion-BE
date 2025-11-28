

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface IUserLogRepository
    {
        Task<PagedResult<UserLog>> GetAllUserLogAsync(
         UserLogSearchRequest request,
         CancellationToken cancellationToken = default);
        Task<PagedResult<UserLog>> GetUserLogByIdAsync(
        Guid actorUserId,
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default);

        Task<PagedResult<UserLog>> GetUserLogByUserIdAsync(
        Guid actorUserId,
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default);
    }
}
