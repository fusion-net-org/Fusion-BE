using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class TaskRepository : GenericRepository<ProjectTask>, ITaskRepository
    {
        private readonly FusionDbContext _context;
        public TaskRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTask task, Guid UserId)
        {
            var project = await _context.Projects.FindAsync(task.ProjectId);

            if (project == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Project"));
     
            var sprint = await _context.Sprints.FindAsync(task.SprintId);
            if (sprint == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

            task.Id = Guid.NewGuid();
            task.CreateAt = DateTime.UtcNow.AddHours(7);
            task.DueDate = DateTime.UtcNow.AddHours(7);
            task.Status = "To Do";
            task.CreatedBy = UserId;

            await _context.ProjectTasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(Guid id)
        {
            var taskById = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskById == null)
            {
                throw CustomExceptionFactory.
                                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));
            }

            return taskById;
        }

        public async Task<PagedResult<ProjectTask>> GetAllTasksAsync(
             PagedRequest request,
             CancellationToken cancellationToken = default)
        {
            var query = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .AsQueryable();


            return await query.ToPagedResultAsync(request, cancellationToken);
        }


        public async Task<ProjectTask?> UpdateTaskAsync(ProjectTask task, Guid userId)
        {
            var existingTask = await _context.ProjectTasks.FindAsync(task.Id);
            if (existingTask == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            if (existingTask.CreatedBy != userId)
                throw CustomExceptionFactory.CreateForbiddenError();

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == task.ProjectId);
            if (project == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Project"));

            var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == task.SprintId);
            if (sprint == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Sprint"));

            existingTask.UpdateAt = DateTime.UtcNow.AddHours(7);

            _context.ProjectTasks.Update(existingTask);
            await _context.SaveChangesAsync();
            return existingTask;
        }



        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            task.Status = "Inactive";

            _context.ProjectTasks.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectTask> ChangeStatus(Guid id, string status, Guid userId)
        {
            var existingTask = await _context.ProjectTasks.FindAsync(id);
            if (existingTask == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            if (existingTask.CreatedBy != userId)
                throw CustomExceptionFactory.CreateForbiddenError();

            existingTask.Status = status;
            existingTask.UpdateAt = DateTime.UtcNow.AddHours(7);

            _context.ProjectTasks.Update(existingTask);
            await _context.SaveChangesAsync();
            return existingTask;
        }
    }
}
