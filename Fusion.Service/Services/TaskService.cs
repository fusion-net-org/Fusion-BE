using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
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
        private readonly ICompanyActivityService _logService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentService _currentService;

        public TaskService(ITaskRepository taskRepository, IMapper mapper, ICompanyActivityService logService)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logService = logService;
        }

        public async Task<ProjectTaskResponse> ChangeStatus(Guid id, string status, Guid userId)
        {
            var entity = await _taskRepository.ChangeStatus(id, status,userId);

            var companyId = await GetCompanyId(entity.ProjectId);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Change status' task",
                Description = $"user id:{_currentService.GetUserId()} has change status of task '{entity.Id}'",
            };
            await _logService.CreateLog(log);
            return _mapper.Map<ProjectTaskResponse>(entity);
        }

        public async Task<ProjectTaskResponse> CreateTaskAsync(ProjectTaskRequest task, Guid UserId)
        {
            var entity = _mapper.Map<ProjectTask>(task);

            var created = await _taskRepository.CreateTaskAsync(entity, UserId);

            var companyId = await GetCompanyId(created.ProjectId);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Create task",
                Description = $"user id:{_currentService.GetUserId()} has created task '{entity.Id}'",
            };
            await _logService.CreateLog(log);

            return _mapper.Map<ProjectTaskResponse>(created);
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            var task = await _unitOfWork.Repository<ProjectTask>().FindAsync( c => c.Id == id);

            var companyId = await GetCompanyId(task.ProjectId);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Delete task",
                Description = $"user id:{_currentService.GetUserId()} has deleted task '{task.Id}'",
            };
            await _logService.CreateLog(log);
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

            var companyId = await GetCompanyId(updatedEntity.ProjectId);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Update task",
                Description = $"user id:{_currentService.GetUserId()} has updated task '{task.Id}'",
            };
            await _logService.CreateLog(log);

            return _mapper.Map<ProjectTaskResponse>(updatedEntity);
        }

        private async Task<Guid> GetCompanyId(Guid? id)
        {
            var project = await _unitOfWork.Repository<Project>().FindAsync(c => c.Id == id);
            var company = await _unitOfWork.Repository<Company>().FindAsync(c => c.Id == project.CompanyId);

            return company.Id;
        }
    }
}
