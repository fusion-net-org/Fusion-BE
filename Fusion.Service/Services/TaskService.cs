using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Task.Request;
using Fusion.Service.ViewModels.Task.Response;

namespace Fusion.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<ProjectTaskResponse> ChangeStatus(Guid id, string status, Guid userId)
        {
            var entity = await _taskRepository.ChangeStatus(id, status,userId);
            return _mapper.Map<ProjectTaskResponse>(entity);
        }

        public async Task<ProjectTaskResponse> CreateTaskAsync(ProjectTaskRequest task, Guid UserId)
        {
            var entity = _mapper.Map<ProjectTask>(task);

            var created = await _taskRepository.CreateTaskAsync(entity, UserId);

            return _mapper.Map<ProjectTaskResponse>(created);
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            return await _taskRepository.DeleteTaskAsync(id);
        }

        public async Task<IEnumerable<ProjectTaskResponse>> GetAllTasksAsync()
        {
            var entity = await _taskRepository.GetAllTasksAsync();

            return _mapper.Map<IEnumerable<ProjectTaskResponse>>(entity);
        }

        public async Task<ProjectTaskResponse?> GetTaskByIdAsync(Guid id)
        {
            var entity = await _taskRepository.GetTaskByIdAsync(id);
            return _mapper.Map<ProjectTaskResponse?>(entity);
        }

        public async Task<ProjectTaskResponse?> UpdateTaskAsync(ProjectTaskRequest task, Guid userId)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(task.Id);
            if (existingTask == null)
                return null;

            _mapper.Map(task, existingTask);

            var updatedEntity = await _taskRepository.UpdateTaskAsync(existingTask, userId);
            return _mapper.Map<ProjectTaskResponse>(updatedEntity);
        }


    }
}
