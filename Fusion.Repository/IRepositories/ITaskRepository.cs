using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ITaskRepository
    {
        Task<ProjectTask> CreateTaskAsync(ProjectTask task, Guid UserId);
        Task<ProjectTask?> GetTaskByIdAsync(Guid id);
        Task<IEnumerable<ProjectTask>> GetAllTasksAsync();
        Task<ProjectTask?> UpdateTaskAsync(ProjectTask task, Guid userId);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<ProjectTask> ChangeStatus(Guid id, string status, Guid userId);
    }
}
