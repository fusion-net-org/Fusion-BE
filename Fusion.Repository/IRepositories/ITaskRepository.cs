using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Task;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.Users;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fusion.Repository.IRepositories
{
    public interface ITaskRepository
    {
        // ===================== EXISTING CRUD =====================
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

        Task<List<ProjectTask>> GetNonBacklogTasksByTicketIdAsync(Guid ticketId, CancellationToken ct = default);

        // ===================== UNIT OF WORK / TRANSACTION =====================
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // ===================== USERS =====================
        Task<bool> UserExistsAsync(Guid userId, CancellationToken ct = default);
        Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct = default);
        Task<Dictionary<Guid, (string? UserName, string? Avatar)>> GetUsersMiniAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);

        // ===================== PROJECTS =====================
        Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken ct = default);
        Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct = default);
        Task<Guid> GetCompanyIdOfProjectAsync(Guid? projectId, CancellationToken ct = default);
        Task<string?> GetProjectCodeAsync(Guid projectId, CancellationToken ct = default);
        Task<Guid?> GetProjectWorkflowIdAsync(Guid projectId, CancellationToken ct = default);

        // ===================== SPRINTS =====================
        Task<Sprint?> GetSprintByIdAsync(Guid sprintId, CancellationToken ct = default);
        Task<bool> SprintExistsAsync(Guid sprintId, CancellationToken ct = default);
        Task<bool> SprintBelongsToProjectAsync(Guid sprintId, Guid projectId, CancellationToken ct = default);
        Task<List<Sprint>> GetSprintsByProjectAsync(Guid projectId, CancellationToken ct = default);

        // ===================== WORKFLOW STATUSES =====================
        Task<WorkflowStatus?> GetWorkflowStatusByIdAsync(Guid statusId, CancellationToken ct = default);
        Task<WorkflowStatus?> FindWorkflowStatusByCodeAsync(string codeOrName, CancellationToken ct = default);
        Task<List<WorkflowStatus>> GetStatusesByWorkflowAsync(Guid workflowId, CancellationToken ct = default);
        Task<WorkflowStatus> ResolveStatusForWorkflowAsync(Guid? statusId, string? codeOrName, Guid workflowId, CancellationToken ct = default);

        // ===================== TASK HELPERS =====================
        Task<int> GetNextOrderInSprintAsync(Guid sprintId, Guid statusId, CancellationToken ct = default);
        Task<long> CountTasksInProjectAsync(Guid projectId, CancellationToken ct = default);
        Task<int> CountChildrenTasksAsync(Guid parentTaskId, CancellationToken ct = default);
        Task<string?> GetTaskTitleAsync(Guid taskId, CancellationToken ct = default);

        // Reorder support (tracked)
        Task<ProjectTask?> GetTaskForUpdateAsync(Guid taskId, CancellationToken ct = default);
        Task<List<ProjectTask>> GetTasksInSprintStatusForUpdateAsync(Guid sprintId, Guid statusId, Guid excludeTaskId, CancellationToken ct = default);

        // Project member
        Task<bool> IsProjectMemberAsync(Guid userId, Guid projectId, CancellationToken ct = default);

        // ===================== ATTACHMENTS =====================
        Task AddAttachmentsAsync(IEnumerable<ProjectTaskAttachment> attachments, CancellationToken ct = default);
        Task<List<ProjectTaskAttachment>> GetTaskAttachmentsAsync(Guid taskId, CancellationToken ct = default);
        Task<ProjectTaskAttachment?> FindAttachmentAsync(Guid taskId, Guid attachmentId, CancellationToken ct = default);
        Task RemoveAttachmentAsync(ProjectTaskAttachment entity, CancellationToken ct = default);

        // Comment attachments: CommentId = long
        Task<List<ProjectTaskAttachment>> GetCommentAttachmentsAsync(IEnumerable<long> commentIds, CancellationToken ct = default);

        // ===================== COMMENTS =====================
        Task<List<Comment>> GetCommentsByTaskIdAsync(Guid taskId, CancellationToken ct = default);
        Task AddCommentAsync(Comment comment, CancellationToken ct = default);

        // ===================== TICKETS =====================
        Task<Ticket?> GetTicketByIdAsync(Guid ticketId, CancellationToken ct = default);
        Task<bool> TicketExistsAsync(Guid ticketId, CancellationToken ct = default);

        //===================== ADMIN =====================
        Task<ProjectTask> GetTaskDetailForAdminByTaskIdAsync(Guid userId, Guid taskId, CancellationToken token = default);
    }
}
