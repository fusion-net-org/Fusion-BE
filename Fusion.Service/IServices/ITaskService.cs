using Fusion.Repository.Bases.Page;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;

namespace Fusion.Service.IServices
{
    public interface ITaskService
    {
        Task<ProjectTaskResponse> CreateTaskAsync(ProjectTaskRequest req, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse?> UpdateTaskAsync(ProjectTaskRequest req, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<ProjectTaskResponse>> GetAllTasksAsync(PagedRequest request, CancellationToken ct = default);
        Task<bool> DeleteTaskAsync(Guid id, Guid userId = default, CancellationToken ct = default);
        Task<ProjectTaskResponse> ChangeStatus(Guid id, string status, Guid userId, CancellationToken ct = default);
    }
}
