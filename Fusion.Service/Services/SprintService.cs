using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Common;
using Fusion.Service.ViewModels.Sprint;
using Fusion.Service.ViewModels.Sprint.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.Services
{
    public interface ISprintService
    {
        Task<SprintVm> CreateAsync(Guid currentUserId, SprintCreateRequest req, CancellationToken ct);
        Task<SprintVm> GetAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task StartAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task AddTasksAsync(Guid sprintId, Guid projectId, IEnumerable<Guid> taskIds, Guid userId, CancellationToken ct);
        Task CompleteAsync(Guid sprintId, Guid projectId, bool carryBacklog, Guid? nextSprintId, CancellationToken ct);
        Task<PagedResult<SprintListItemVm>> GetProjectSprintsAsync(Guid projectId, SprintQuery q, CancellationToken ct);
        Task<SprintDetailVm?> GetProjectSprintDetailAsync(Guid projectId, Guid sprintId, CancellationToken ct);
        Task<SprintChartsVm> GetChartsDataAsync(Guid projectId, CancellationToken ct);

    }
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _repo;
        private readonly ICompanyActivityService _logService;
        private readonly IUnitOfWork _unitOfWork;


        public SprintService(ISprintRepository repo, ICompanyActivityService logService, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _logService = logService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<SprintListItemVm>> GetProjectSprintsAsync(Guid projectId, SprintQuery q, CancellationToken ct)
        {
            var baseQ = _repo.QueryByProject(projectId); // đã lọc IsDeleted + Include Project

            if (!string.IsNullOrWhiteSpace(q.Q))
            {
                var kw = q.Q.Trim().ToLower();
                baseQ = baseQ.Where(x =>
                    (x.Name ?? "").ToLower().Contains(kw) ||
                    (x.Project!.Name ?? "").ToLower().Contains(kw));
            }

            if (q.Statuses?.Count > 0)
            {
                var set = q.Statuses!.ToHashSet();
                baseQ = baseQ.Where(x => set.Contains(x.Status));
            }

            if (q.DateFrom.HasValue) baseQ = baseQ.Where(x => x.StartDate >= q.DateFrom);
            if (q.DateTo.HasValue) baseQ = baseQ.Where(x => x.EndDate <= q.DateTo);

            baseQ = (q.SortColumn?.ToLower()) switch
            {
                "name" => (q.SortDescending ? baseQ.OrderByDescending(x => x.Name) : baseQ.OrderBy(x => x.Name)),
                "start_date" => (q.SortDescending ? baseQ.OrderByDescending(x => x.StartDate) : baseQ.OrderBy(x => x.StartDate)),
                "end_date" => (q.SortDescending ? baseQ.OrderByDescending(x => x.EndDate) : baseQ.OrderBy(x => x.EndDate)),
                "created_at" => (q.SortDescending ? baseQ.OrderByDescending(x => x.CreatedAt) : baseQ.OrderBy(x => x.CreatedAt)),
                _ => (q.SortDescending ? baseQ.OrderByDescending(x => x.StartDate) : baseQ.OrderBy(x => x.StartDate))
            };

            var total = await baseQ.CountAsync(ct);
            var page = Math.Max(1, q.PageNumber);
            var size = Math.Max(1, q.PageSize);

            var items = await baseQ.Skip((page - 1) * size).Take(size)
                .Select(x => new SprintListItemVm
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    ProjectName = x.Project!.Name!,
                    Name = x.Name ?? "",
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status.ToString(),
                    Color = x.Color
                })
                .ToListAsync(ct);

            return new PagedResult<SprintListItemVm>(items, total, page, size);
        }

        public async Task<SprintDetailVm?> GetProjectSprintDetailAsync(Guid projectId, Guid sprintId, CancellationToken ct)
        {
            var s = await _repo.QueryByProject(projectId)
                .Where(x => x.Id == sprintId)
                .Select(x => new SprintDetailVm
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    ProjectName = x.Project!.Name!,
                    Name = x.Name ?? "",
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status.ToString(),
                    Color = x.Color,
                    Goal = x.Goal,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                })
                .FirstOrDefaultAsync(ct);

            return s;
        }
        public async Task<SprintVm> CreateAsync(Guid currentUserId, SprintCreateRequest req, CancellationToken ct)
        {
            if (req.DurationWeeks is not (1 or 2))
                throw CustomExceptionFactory.CreateBadRequestError("DurationWeeks must be 1 or 2");


            var start = req.StartDate.Date;
            var end = start.AddDays(req.DurationWeeks * 7); // exclusive end


            if (await _repo.IsOverlappedAsync(req.ProjectId, start, end, null, ct))
                throw CustomExceptionFactory.CreateBadRequestError("Time range overlaps other sprint in project");


            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = req.ProjectId,
                Name = string.IsNullOrWhiteSpace(req.Name) ? $"Sprint {start:yyyy-MM-dd}" : req.Name!.Trim(),
                Goal = req.Goal,
                StartDate = start,
                EndDate = end,
                Status = SprintStatus.Planning,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(sprint, req.TaskIds, currentUserId, ct);

            var project = await _unitOfWork.Repository<Project>().FindAsync(c => c.Id == sprint.ProjectId);
            var company = await _unitOfWork.Repository<Company>().FindAsync(c => c.Id == project.CompanyId);

            var currentUserName = await GetUserName(currentUserId);
            var log = new CompanyActivityLog
            {
                CompanyId = company.Id,
                ActorUserId = currentUserId,
                Title = "Create sprint",
                Description = $"user:{currentUserName} has created sprint '{sprint.Name}' for project '{project.Name}''",
            };
            await _logService.CreateLog(log);

            return new SprintVm
            {
                Id = created.Id,
                ProjectId = created.ProjectId,
                Name = created.Name,
                StartDate = created.StartDate,
                EndDate = created.EndDate,
                Status = (byte)created.Status,
                TaskCount = created.ProjectTasks?.Count ?? 0
            };
        }
        public async Task<SprintVm> GetAsync(Guid sprintId, Guid projectId, CancellationToken ct)
        {
            var data = await _repo.GetVmAsync(sprintId, projectId, ct) ?? throw CustomExceptionFactory.CreateNotFoundError("Sprint not found");
            return new SprintVm
            {
                Id = data.Id,
                ProjectId = data.ProjectId,
                Name = data.Name,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                Status = (byte)data.Status,
                TaskCount = data.TaskCount
            };
        }


        public Task StartAsync(Guid sprintId, Guid projectId, CancellationToken ct)
        => _repo.StartAsync(sprintId, projectId, ct);


        public Task AddTasksAsync(Guid sprintId, Guid projectId, IEnumerable<Guid> taskIds, Guid userId, CancellationToken ct)
        => _repo.AddTasksAsync(sprintId, projectId, taskIds, userId, ct);


        public Task CompleteAsync(Guid sprintId, Guid projectId, bool carryBacklog, Guid? nextSprintId, CancellationToken ct)
        => _repo.CompleteAsync(sprintId, projectId, carryBacklog, nextSprintId, ct);

        private async Task<string?> GetUserName(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().FindAsync(c => c.Id == userId);
            return user.UserName;
        }

        public async Task<SprintChartsVm> GetChartsDataAsync(Guid projectId, CancellationToken ct)
        {
            var sprints = await _repo.QueryByProject(projectId)
                .OrderBy(s => s.StartDate)
                .Include(s => s.ProjectTasks)
                .ToListAsync(ct);

            var statusDistribution = sprints
                .GroupBy(s => s.Status)
                .Select(g => new SprintStatusDistributionDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var sprintWorkload = sprints.Select(s =>
            {
                var tasks = s.ProjectTasks.Where(t => !t.IsDeleted);

                return new SprintChartDto
                {
                    SprintName = s.Name ?? "",
                    EstimatedHours = tasks.Sum(t => t.EstimateHours ?? 0),
                    RemainingHours = tasks.Sum(t => t.RemainingHours ?? 0),
                    TodoCount = tasks.Count(t => t.Status == "To Do"),
                    InProgressCount = tasks.Count(t => t.Status == "In Progress"),
                    DoneCount = tasks.Count(t => t.Status == "Done"),
                    Review = tasks.Count(t => t.Status == "Review")
                };
            }).ToList();

            return new SprintChartsVm
            {
                StatusDistribution = statusDistribution,
                SprintWorkload = sprintWorkload
            };
        }

    }
}
