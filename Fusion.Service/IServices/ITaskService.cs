using Fusion.Repository.Bases.Page;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;

namespace Fusion.Service.IServices
{
    public interface ITaskService
    {
        Task<ProjectTaskResponse> CreateTaskAsync(ProjectTaskRequest task, Guid UserId);
        Task<ProjectTaskResponse?> GetTaskByIdAsync(Guid id);
        Task<PagedResult<ProjectTaskResponse>> GetAllTasksAsync(PagedRequest request, CancellationToken cancellationToken = default);
        Task<ProjectTaskResponse?> UpdateTaskAsync(ProjectTaskRequest task, Guid userId);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<ProjectTaskResponse> ChangeStatus(Guid id, string status, Guid userId);

    }
}
