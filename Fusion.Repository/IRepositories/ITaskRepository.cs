using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.Users;

namespace Fusion.Repository.IRepositories
{
    public interface ITaskRepository
    {
        Task<ProjectTask> AddAsync(ProjectTask entity, CancellationToken ct = default);
        Task<ProjectTask?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<ProjectTask>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
        Task<ProjectTask> UpdateAsync(ProjectTask entity, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<ProjectTask>> GetTasksBySprintIdAsync(Guid sprintId, TaskBySprintRequest request, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);


        Task<PagedResult<ProjectTask>> GetAllTaskByUserId(Guid userId, TaskFilterRequest request, CancellationToken token = default);

        Task<ProjectTask> GetTaskDetailByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default);

        Task<List<Guid>> GetMemberIdByTaskId(Guid taskId, CancellationToken token = default);

        Task<List<ProjectTask>> GetSubTasksByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default);

        Task<List<ProjectTask>> GetTasksAssignedToUserAsync(Guid userId, CancellationToken token = default);

        Task<UserTaskDashBoard> GetUserTaskDashboardAsync(Guid userId, CancellationToken token = default);

    }
}
