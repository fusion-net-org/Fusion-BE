using Azure;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Sprint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public interface ISprintService
    {
        Task<SprintVm> CreateAsync(Guid currentUserId, SprintCreateRequest req, CancellationToken ct);
        Task<SprintVm> GetAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task StartAsync(Guid sprintId, Guid projectId, CancellationToken ct);
        Task AddTasksAsync(Guid sprintId, Guid projectId, IEnumerable<Guid> taskIds, Guid userId, CancellationToken ct);
        Task CompleteAsync(Guid sprintId, Guid projectId, bool carryBacklog, Guid? nextSprintId, CancellationToken ct);
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
            var log = new CompanyActivityLog
            {
                CompanyId = company.Id,
                ActorUserId = currentUserId,
                Title = "Create sprint",
                Description = $"user id:{currentUserId} has created sprint '{sprint.Id}' for project '{sprint.ProjectId}''",
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
    }
}
