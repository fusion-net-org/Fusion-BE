using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;
using Microsoft.AspNetCore.Http;

namespace Fusion.Service.IServices
{
    public interface ITaskService
    {
        Task<ProjectTaskResponse> CreateTaskAsync(ProjectTaskRequest req, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse?> UpdateTaskAsync(ProjectTaskRequest req, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<ProjectTaskResponse>> GetAllTasksAsync(PagedRequest request, CancellationToken ct = default);
        Task<bool> DeleteTaskAsync(Guid id, Guid userId = default, CancellationToken ct = default);
        Task<PagedResult<ProjectTaskResponse>> GetTasksBySprintIdAsync(Guid sprintId, TaskBySprintRequest request, CancellationToken ct = default);

        //---------------------------------------------------------------------------
        Task<ProjectTaskResponse> ChangeStatus(Guid id, string statusText, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse> ChangeStatusById(Guid id, Guid statusId, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse> ReorderAsync(Guid projectId, Guid sprintId, Guid taskId, Guid toStatusId, int toIndex, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse> MoveToSprintAsync(Guid taskId, Guid toSprintId, Guid userId, CancellationToken ct = default);
        Task<ProjectTaskResponse> MarkDoneAsync(Guid taskId, Guid userId, CancellationToken ct = default);
        Task<SplitTaskResponse> SplitAsync(Guid taskId, Guid userId, CancellationToken ct = default);

        Task<IReadOnlyList<TaskAttachmentResponse>> UploadAttachmentsAsync(
      Guid taskId,
      IReadOnlyList<IFormFile> files,
      string? description,
      Guid userId,
      CancellationToken ct = default);

        Task<IReadOnlyList<TaskAttachmentResponse>> GetAttachmentsAsync(
            Guid taskId,
            CancellationToken ct = default);

        Task<bool> DeleteAttachmentAsync(
            Guid taskId,
            Guid attachmentId,
            Guid userId,
            CancellationToken ct = default);
    }
}
