using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;

namespace Fusion.Service.IServices
{
    public interface ITaskService
    {
        Task<ProjectTaskResponse> CreateTaskAsync(ProjectTaskRequest task, Guid UserId);
        Task<ProjectTaskResponse?> GetTaskByIdAsync(Guid id);
        Task<IEnumerable<ProjectTaskResponse>> GetAllTasksAsync();
        Task<ProjectTaskResponse?> UpdateTaskAsync(ProjectTaskRequest task, Guid userId);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<ProjectTaskResponse> ChangeStatus(Guid id, string status, Guid userId);

    }
}
