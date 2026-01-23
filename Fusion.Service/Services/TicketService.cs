using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class TicketService : ITicketService
    {
        private readonly IMapper _mapper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<TicketRequest> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyActivityService _logService;
        private readonly ICurrentService _currentService;
        private readonly IProjectService _projectService;
        private readonly IWorkflowStatusRepository _workflowStatusRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectComponentRepository _projectComponentRepository;
        private readonly ITicketHistoryRepository _ticketHistoryRepository;

        public TicketService(IMapper mapper, ITicketRepository ticketRepository, IUserRepository userRepository, IValidator<TicketRequest> validator,
            IUnitOfWork unitOfWork, ICompanyActivityService logService, ICurrentService currentService, IProjectService projectService, IWorkflowStatusRepository workflowStatusRepository, ITaskRepository taskRepository, IProjectRepository projectRepository,IProjectComponentRepository projectComponentRepository, ITicketHistoryRepository ticketHistoryRepository)
        {
            _mapper = mapper;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _currentService = currentService;
            _projectService = projectService;
            _workflowStatusRepository = workflowStatusRepository;
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _projectComponentRepository = projectComponentRepository;
            _ticketHistoryRepository = ticketHistoryRepository;
        }

        public async Task<TicketResponse?> CreateTicketAsync(TicketRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            await _validator.ValidateAndThrowAsync(
               request,
               opts => opts.IncludeRuleSets("Create"),
               cancellationToken
               );

            //if (!request.StatusId.HasValue)
            //    throw CustomExceptionFactory.CreateBadRequestError("Workflow status is required.");

            //var statusExists = await _workflowStatusRepository.ExistsAsync(request.StatusId.Value);
            //if (!statusExists)
            //    throw CustomExceptionFactory.CreateBadRequestError("Workflow status is invalid or does not exist.");

            Project? project = null;
            if (request.ProjectId.HasValue)
            {
                project = await _projectRepository.GetProjectById(
                    request.ProjectId.Value, cancellationToken);

                if (project == null)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "Project does not exist.");
            }

            if (request.DueDate.HasValue &&
              request.DueDate <= DateTime.UtcNow.AddHours(7))
            {
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Due date must be in the future.");
            }


            if (project?.IsMaintenance == true)
            {
                if (!request.ComponentId.HasValue)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "Component is required for maintenance project.");

                var component = await _projectComponentRepository
                    .GetByIdAsync(request.ComponentId.Value, cancellationToken);

                if (component == null)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "Component does not exist.");

                if (component.ProjectId != project.Id)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "Component does not belong to this project.");
            }

            var ticket = _mapper.Map<Ticket>(request);

            ticket.TicketType = request.TicketType.ToString();
            ticket.ComponentId = request.ComponentId;
            ticket.SubmittedBy = request.SubmittedBy;

            ticket.TicketCode = await GenerateUniqueTicketCodeAsync();


            var newTicket = await _ticketRepository.AddTicketAsync(ticket, cancellationToken);

            var currentUserId = _currentService.GetUserId();

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = newTicket.Id,
                Action = TicketHistoryAction.Created.ToString(),
                Description = "Ticket was created with status Pending",
                PerformedBy = currentUserId
            }, cancellationToken);

            return _mapper.Map<TicketResponse>(newTicket);
        }

        public async Task<bool?> DeleteTicketAsync(Guid ticketId, string reason, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var currentUserId = _currentService.GetUserId();

            if (ticket.SubmittedBy != currentUserId)
                throw new UnauthorizedAccessException("You are not allowed to delete this ticket");

            await _ticketRepository.DeleteTicketAsync(ticket, reason, cancellationToken);
            return true;
        }


        public async Task<TicketPagedResponse> GetPageTicketshAsync(
            TicketPagedSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var fullQuery = _ticketRepository.BuildTicketQuery(request);

            var fullData = await fullQuery.ToListAsync(cancellationToken);

            var statusCounts = fullData
                .GroupBy(t => t.status ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var totalFull = fullData.Count;

            var paged = await fullQuery.ToPagedResultAsync(request, cancellationToken);

            return new TicketPagedResponse
            {
                PageData = new PagedResult<TicketResponse>
                {
                    Items = _mapper.Map<List<TicketResponse>>(paged.Items),
                    TotalCount = paged.TotalCount,
                    PageNumber = paged.PageNumber,
                    PageSize = paged.PageSize
                },
                StatusCounts = statusCounts,
                Total = totalFull
            };
        }

        public async Task<TicketPagedResponse> GetPageTicketAdminAsync(
            TicketPagedSearchRequest request,
            Guid AdminId,
            CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetUserByIdAsync(AdminId);

            if (admin?.IsSystemAdmin != true)
                throw CustomExceptionFactory.CreateNotFoundError("Admin does not exist in this system");

            var fullQuery = _ticketRepository.BuildTicketQuery(request);

            var fullData = await fullQuery.ToListAsync(cancellationToken);

            var statusCounts = fullData
                .GroupBy(t => t.status ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var totalFull = fullData.Count;

            var paged = await fullQuery.ToPagedResultAsync(request, cancellationToken);

            return new TicketPagedResponse
            {
                PageData = new PagedResult<TicketResponse>
                {
                    Items = _mapper.Map<List<TicketResponse>>(paged.Items),
                    TotalCount = paged.TotalCount,
                    PageNumber = paged.PageNumber,
                    PageSize = paged.PageSize
                },
                StatusCounts = statusCounts,
                Total = totalFull
            };
        }



        public async Task<TicketProcessSummaryResponse?> BuildTicketProcessAsync(
     Guid ticketId,
     CancellationToken ct)
        {
            // 1. Lấy tất cả task non-backlog gắn với ticket
            var tasks = await _taskRepository.GetNonBacklogTasksByTicketIdAsync(ticketId, ct);

            // Không có task non-backlog => chưa có execution
            if (tasks == null || tasks.Count == 0)
                return null;

            var items = new List<TicketProcessItemResponse>();

            var totalNonBacklog = tasks.Count;

            int doneCount = 0;
            int startedCount = 0;              // số task đã "bắt đầu xử lý"
            DateTimeOffset? firstStartedAt = null;
            DateTimeOffset? lastDoneAt = null;

            foreach (var task in tasks)
            {
                var status = task.CurrentStatus;   // trạng thái workflow hiện tại

                var statusName = status?.Name ?? "Not started";
                var statusCategory = status?.Category ?? string.Empty;

                // task được coi là "Done" nếu CurrentStatus.IsEnd = true
                var isDone = status?.IsEnd == true;

                // task được coi là "đã bắt đầu xử lý" nếu có CurrentStatus (khác null)
                // tuỳ nghiệp vụ của bạn, có thể tighten điều kiện (ví dụ chỉ tính những status Category = "InProgress")
                var isStarted = status != null;

                DateTimeOffset? startedAt = null;
                DateTimeOffset? doneAt = null;

                // thời điểm bắt đầu: tạm lấy theo CreateAt (hoặc UpdateAt nếu CreateAt null)
                if (isStarted)
                {
                    startedCount++;

                    startedAt = ToUtcOffset(task.CreateAt ?? task.UpdateAt ?? DateTime.UtcNow);

                    if (firstStartedAt == null || startedAt < firstStartedAt)
                        firstStartedAt = startedAt;
                }

                // thời điểm hoàn thành: tạm lấy theo UpdateAt nếu đang ở trạng thái End
                if (isDone)
                {
                    doneCount++;

                    doneAt = ToUtcOffset(task.UpdateAt ?? task.CreateAt ?? DateTime.UtcNow);

                    if (lastDoneAt == null || doneAt > lastDoneAt)
                        lastDoneAt = doneAt;
                }

                // lần cuối cùng "động" tới task: dùng UpdateAt (fallback CreateAt)
                var lastMovedAt = ToUtcOffset(task.UpdateAt ?? task.CreateAt ?? DateTime.UtcNow);

                items.Add(new TicketProcessItemResponse
                {
                    TaskId = task.Id,
                    TaskCode = task.Code ?? string.Empty,
                    Title = task.Title ?? string.Empty,
                    StatusName = statusName,
                    StatusCategory = statusCategory,
                    IsDone = isDone,
                    StartedAt = startedAt,
                    LastMovedAt = lastMovedAt,
                    DoneAt = doneAt
                });
            }

            if (items.Count == 0)
                return null;

            // % tiến độ = số task Done / tổng task non-backlog
            var progress = totalNonBacklog == 0
                ? 0m
                : Math.Round((decimal)doneCount * 100m / totalNonBacklog, 2);


            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket != null)
            {
                if (startedCount > 0 && progress < 100)
                {
                    ticket.ResolvedAt = firstStartedAt?.UtcDateTime;
                }

                if (progress == 100)
                {
                    ticket.ClosedAt = lastDoneAt?.UtcDateTime;
                }
                else if (progress < 100)
                {
                    ticket.ClosedAt = null;

                }

                ticket.UpdatedAt = DateTime.UtcNow;

                await _ticketRepository.UpdateTicketAsync(ticket.Id, ticket, ct);
            }

            return new TicketProcessSummaryResponse
            {
                HasExecution = true,
                TotalNonBacklogTasks = totalNonBacklog,
                StartedCount = startedCount,
                DoneCount = doneCount,
                ProgressPercent = progress,
                FirstStartedAt = firstStartedAt,
                LastDoneAt = lastDoneAt,
                Items = items
            };
        }

        // Giữ helper này dạng nullable cho tiện xài với CreateAt/UpdateAt
        private static DateTimeOffset ToUtcOffset(DateTime? dt)
        {
            var value = dt ?? DateTime.UtcNow;

            if (value.Kind == DateTimeKind.Unspecified)
                return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));

            return new DateTimeOffset(value.ToUniversalTime());
        }




        public async Task<TicketResponse?> GetTicketByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Ticket"));

            var dto = _mapper.Map<TicketResponse>(ticket);

            //build thêm Process từ các task non-backlog
            dto.Process = await BuildTicketProcessAsync(ticket.Id, cancellationToken);

            var tasks = ticket.Tasks
                .Where(t => !t.IsDeleted)
                .ToList();

            dto.TotalTask = tasks.Count;
            dto.TotalTaskClosed = tasks.Count(t => t.IsClose);
            dto.TotalTaskNotClose = tasks.Count(t => !t.IsClose);

            var totalTask = tasks.Count;

            dto.ProcessTicketIsClose = totalTask == 0
                ? 0
                : (int)Math.Round(
                    (double)dto.TotalTaskClosed / totalTask * 100
                  );

            return dto;
        }

        public async Task<TicketResponseV2?> GetTicketByIdAsyncV2(Guid id, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Ticket"));

            var dto = _mapper.Map<TicketResponseV2>(ticket);

            dto.Process = await BuildTicketProcessAsync(ticket.Id, cancellationToken);

            return dto;
        }


        public async Task<TicketDashboardResponse> GetTicketDashboardAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var tickets = await _ticketRepository.GetTicketsForDashboardAsync(projectId, cancellationToken);
            var dashboard = new TicketDashboardResponse();

            // Ticket Status
            var statusInProgress = tickets.Count(t => t.WorkflowStatus != null && t.WorkflowStatus.IsStart);
            var statusResolved = tickets.Count(t => t.WorkflowStatus != null && t.WorkflowStatus.IsEnd);
            dashboard.TicketStatusData.Add(new TicketStatusChartItem { Name = "In Progress", Value = statusInProgress });
            dashboard.TicketStatusData.Add(new TicketStatusChartItem { Name = "Resolved", Value = statusResolved });

            // Budget theo Priority
            dashboard.BudgetByPriority = tickets
                .GroupBy(t => t.Priority ?? "Unknown")
                .Select(g => new BudgetByPriorityItem { Status = g.Key, Budget = g.Sum(t => t.Budget ?? 0) })
                .ToList();

            // Số lượng Priority
            dashboard.TicketPriorityData = tickets
                .GroupBy(t => t.Priority ?? "Unknown")
                .Select(g => new TicketPriorityChartItem { Priority = g.Key, Value = g.Count() })
                .ToList();

            // Số lượng Resolved và Closed
            var resolved = tickets.Count(t => t.ResolvedAt.HasValue);
            var closed = tickets.Count(t => t.ClosedAt.HasValue);
            dashboard.ResolvedAndClosedData.Add(new ResolvedClosedChartItem { Name = "Resolved", Value = resolved });
            dashboard.ResolvedAndClosedData.Add(new ResolvedClosedChartItem { Name = "Closed", Value = closed });

            // Resolved & Closed timeline theo tuần
            dashboard.ResolvedClosedTimeline = tickets
                .Where(t => t.ResolvedAt.HasValue || t.ClosedAt.HasValue)
                .GroupBy(t => t.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ResolvedClosedTimelineItem
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Resolved = g.Count(t => t.ResolvedAt.HasValue),
                    Closed = g.Count(t => t.ClosedAt.HasValue)
                })
                .ToList();

            return dashboard;
        }


        public async Task<PagedResult<TicketResponse>> GetTicketsByProjectIdAsync(TicketByProjectPagedRequest request, CancellationToken cancellationToken = default)
        {
            var tickets = await _ticketRepository.GetTicketsByProjectIdAsync(request, cancellationToken);

            var mapped = _mapper.Map<List<TicketResponse>>(tickets.Items);

            return new PagedResult<TicketResponse>
            {
                Items = mapped,
                TotalCount = tickets.TotalCount,
                PageNumber = tickets.PageNumber,
                PageSize = tickets.PageSize
            };
        }
        public async Task<bool?> RestoreTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var currentUserId = _currentService.GetUserId();

            if (ticket.SubmittedBy != currentUserId)
                throw new UnauthorizedAccessException("You are not allowed to restore this ticket");

            if ((bool)!ticket.IsDeleted)
                throw new InvalidOperationException("Ticket is not deleted");

            await _ticketRepository.RestoreTicketAsync(ticket, cancellationToken);
            return true;
        }
        public async Task<TicketStatusCountResponse> GetTicketStatusCountAsync(
              Guid? projectId = null,
              Guid? companyRequestId = null,
              Guid? companyExecutorId = null,
              CancellationToken cancellationToken = default)
        {
            var result = await _ticketRepository.GetTicketStatusCountAsync(
                projectId,
                companyRequestId,
                companyExecutorId,
                cancellationToken
            );

            return result;
        }

        public async Task<TicketResponse?> AcceptTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var currentUserId = _currentService.GetUserId();

            if (ticket.status != TicketStatusEnum.Pending.ToString())
                throw new InvalidOperationException("Only tickets with status Pending can be accepted.");

            var updatedTicket = await _ticketRepository.AcceptTicketAsync(ticketId, cancellationToken);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = ticketId,
                Action = TicketHistoryAction.Accepted.ToString(),
                Description = "Ticket was accepted",
                PerformedBy = currentUserId
            }, cancellationToken);


            return _mapper.Map<TicketResponse>(updatedTicket);
        }

        public async Task<TicketResponse?> RejectTicketAsync(Guid ticketId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var currentUserId = _currentService.GetUserId();


            if (ticket.status != TicketStatusEnum.Pending.ToString())
                throw new InvalidOperationException("Only tickets with status Pending can be accepted.");

            var updatedTicket = await _ticketRepository.RejectTicketAsync(ticketId, reason, cancellationToken);
            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = ticketId,
                Action = TicketHistoryAction.Rejected.ToString(),
                Description = $"Ticket was rejected. Reason: {reason}",
                PerformedBy = currentUserId
            });
            return _mapper.Map<TicketResponse>(updatedTicket);
        }

        public async Task<TicketResponse?> UpdateTicketAsync(TicketRequest request, Guid ticketId, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Update"),
                cancellationToken);

            var result = await _ticketRepository.UpdateTicketAsync(ticketId, _mapper.Map<Ticket>(request), cancellationToken);


            return _mapper.Map<TicketResponse>(result);
        }

        private async Task<Guid> GetCompanyIdAsync(Guid id)
        {
            var project = await _unitOfWork.Repository<Project>().FindAsync(c => c.Id == id);
            var company = await _unitOfWork.Repository<Company>().FindAsync(c => c.Id == project.CompanyId);

            return company.Id;
        }
        private async Task<string?> GetUserName(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().FindAsync(c => c.Id == userId);
            return user.UserName;
        }

        private async Task<string> GenerateUniqueTicketCodeAsync()
        {
            string code;
            do
            {
                code = TicketCodeGenerator.Generate("TK");
            }
            while (await _ticketRepository.ExistsByCodeAsync(code));

            return code;
        }

        public async Task<TicketResponse?> CloseTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            if ((bool)ticket.IsClose)
                throw new InvalidOperationException("Ticket is already closed");

            var currentUserId = _currentService.GetUserId();

            var updatedTicket = await _ticketRepository.CloseTicketAsync(ticketId, cancellationToken);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = ticketId,
                Action = TicketHistoryAction.Closed.ToString(),
                Description = "Ticket was closed",
                PerformedBy = currentUserId
            }, cancellationToken);

            return _mapper.Map<TicketResponse>(updatedTicket);
        }
        public async Task<TicketResponse?> RequestCloseTicketAsync(
      Guid ticketId,
      CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            if (ticket.status != TicketStatusEnum.Accepted.ToString())
                throw new InvalidOperationException(
                    "Only accepted tickets can request close.");

            if (ticket.IsClose == true)
                throw new InvalidOperationException("Ticket is already closed.");

            var hasOpenTask = ticket.Tasks
                .Any(t => !t.IsDeleted && !t.IsClose);

            if (hasOpenTask)
                throw new InvalidOperationException(
                    "All tasks must be closed before requesting ticket close.");

            if (ticket.status == TicketStatusEnum.WaitingForCloseApproval.ToString())
                throw new InvalidOperationException(
                    "Ticket is already waiting for close approval.");

            var currentUserId = _currentService.GetUserId();

            var updatedTicket = await _ticketRepository
                .RequestCloseTicketAsync(ticketId, cancellationToken);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = ticketId,
                Action = TicketHistoryAction.RequestClose.ToString(),
                Description = "Executor requested to close ticket",
                PerformedBy = currentUserId
            }, cancellationToken);

            return _mapper.Map<TicketResponse>(updatedTicket);
        }

        public async Task<TicketResponse?> AcceptCloseTicketAsync(
          Guid ticketId,
          CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            if (ticket.status != TicketStatusEnum.WaitingForCloseApproval.ToString())
                throw new InvalidOperationException(
                    "Ticket is not waiting for close approval.");

            var currentUserId = _currentService.GetUserId();

            var updatedTicket = await _ticketRepository
                .AcceptCloseTicketAsync(ticketId, cancellationToken);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = ticketId,
                Action = TicketHistoryAction.Closed.ToString(),
                Description = "Requester accepted close ticket",
                PerformedBy = currentUserId
            }, cancellationToken);

            return _mapper.Map<TicketResponse>(updatedTicket);
        }
        public async Task<TicketResponse?> RejectCloseTicketAsync(
            Guid ticketId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Reject reason is required.");

            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            if (ticket.status != TicketStatusEnum.WaitingForCloseApproval.ToString())
                throw new InvalidOperationException(
                    "Ticket is not waiting for close approval.");

            var currentUserId = _currentService.GetUserId();

            var updatedTicket = await _ticketRepository
                .RejectCloseTicketAsync(ticketId, reason, cancellationToken);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = ticketId,
                Action = TicketHistoryAction.Rejected.ToString(),
                Description = $"Requester rejected close request. Reason: {reason}",
                PerformedBy = currentUserId
            }, cancellationToken);

            return _mapper.Map<TicketResponse>(updatedTicket);
        }



    }
    public static class TicketCodeGenerator
    {
        public static string Generate(string prefix = "TK")
        {
            var random = Random.Shared.Next(100000, 999999);
            return $"{prefix}-{random}";
        }
    }
}
