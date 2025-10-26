using AutoMapper;
using Azure;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectRequest;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class ProjectRequestService : IProjectRequestService
    {
        private readonly IProjectRequestRepository _projectRequestRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ICurrentService _currentService;
        private readonly ICompanyActivityService _logService;
        private readonly IMailService _mailService;
        private readonly IUnitOfWork _unitOfWork;
        public ProjectRequestService(IProjectRequestRepository projectRequestRepository, INotificationService notificationService, IMapper mapper, ICurrentService currentService, ICompanyActivityService logService, IMailService mailService, IUnitOfWork unitOfWork)
        {
            _projectRequestRepository = projectRequestRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _currentService = currentService;
            _logService = logService;
            _mailService = mailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectRequestResponse> AcceptProjectRequestAsync(Guid requestId, string executorEmail, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.AcceptProjectRequestAsync(requestId, executorEmail, cancellationToken);

            //await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            //{
            //    UserId = result.RequesterCompany.OwnerUser.Id, // người nhận notification
            //    Title = "Project Request Accept",
            //    Body = $"{result.ExecutorCompany.OwnerUser.UserName} from {result.ExecutorCompany.Name} has accpeted your project request: \"{result.Name}\".",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "PROJECT_REQUEST_ACCEPT",
            //    Context = result.Project.Name,
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString(),
            //});
            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = result.RequesterCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Accept project request",
                Description = $"User: '{currentUserName}' has accepted project request {result.Code} for project {result.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);

            return _mapper.Map<ProjectRequestResponse>(result);
        }

        public async Task<ProjectRequestResponse> AddProjectRequestAsync(CreateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken)
        {
            var projectRequest = _mapper.Map<ProjectRequest>(request);
            var code = ProjectCodeUtil.GenerateProjectRequestCode();
            var response = await _projectRequestRepository.AddProjectRequestAsync(projectRequest, vendorEmail, code, cancellationToken);

            //await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            //{
            //    UserId = response.ExecutorCompany.OwnerUser.Id, // Send to the invited user
            //    Title = "You have been hired for a project",
            //    Body = $"{response.RequesterCompany.OwnerUser.UserName} from {response.RequesterCompany.Name} requested your company to execute a project. We can discus for having a good cooperation",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "CREATE_PROJECT_REQUEST",
            //    Context = null,
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString(),
            //});
            var currentUserName = await GetUserName(_currentService.GetUserId());

            var emailBody = $@"
                <p>Dear {response.ExecutorCompany.OwnerUser.UserName},</p>
               <p><b>{response.RequesterCompany.Name}</b> has invited your company to collaborate on the project <b>“{response.Name}”</b>.</p>
                <p>Please log in to the Fusion platform to view and respond to the request.</p>
                <br/>
                <p>Best regards,<br/><b>Fusion System</b></p>
            ";

            await _mailService.SendEmailAsync(new ViewModels.Companies.Email.MailRequest()
            {
                Subject = $"New Project Collaboration Request from {response.RequesterCompany.Name}",
                Body = emailBody,
                ToEmail = response.ExecutorCompany.OwnerUser.Email
            });

            var log = new CompanyActivityLog
            {
                CompanyId = request.ExecutorCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Create project request",
                Description = $"User:'{currentUserName}' has created project request {response.Code} for project {response.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<ProjectRequestResponse>(response);
        }

        public async Task<bool> DeleteProjectRequestAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.DeleteProjectRequestAsync(id, cancellationToken);

            var projectRequest = await _projectRequestRepository.GetProjectRequestByIdAsync(id);
            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = projectRequest?.RequesterCompany.Id ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Delete project request",
                Description = $"User:'{currentUserName}' has deleted project request {projectRequest?.Code} for project {projectRequest?.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);
            return result;
        }

        public async Task<ProjectRequestResponse?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.GetProjectRequestByIdAsync(id, cancellationToken);
            return _mapper?.Map<ProjectRequestResponse?>(result);
        }

        public async Task<ProjectRequestRejectResponse> RejectProjectRequestAsync(Guid requestId, string executorEmail, string reason, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.RejectProjectRequestAsync(requestId, executorEmail, cancellationToken);

            if (!result)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.FAILED.FormatMessage("Reject project request failed"));

            var projectRequest = await _projectRequestRepository.GetProjectRequestByIdAsync(requestId);

            //await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            //{
            //    UserId = projectRequest.RequesterCompany.OwnerUser.Id, // người nhận notification
            //    Title = "Project Request Rejected",
            //    Body = $"{projectRequest.ExecutorCompany.OwnerUser.UserName} from {projectRequest.ExecutorCompany.Name} has rejected your project request: \"{projectRequest.Name}\". Reason: {reason}.",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "PROJECT_REQUEST_REJECT",
            //    Context = null,
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString(),
            //});

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = projectRequest?.RequesterCompany.Id ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Reject project request",
                Description = $"User:'{currentUserName}' has rejected project request {projectRequest?.Code} for project {projectRequest?.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);
            return new ProjectRequestRejectResponse
            {
                Reason = reason,
                RejectedAt = DateTime.UtcNow.AddHours(7),
                RejectedBy = executorEmail,
                RequestId = requestId,
                Status = ProjectRequestStatusEnum.Rejected.ToString(),
            };
        }

        public async Task<PagedResult<ProjectRequestResponse>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, CancellationToken cancellationToken = default)
        {
            if (filter == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _projectRequestRepository.SearchProjectRequestAsync(filter, userCompanyId, cancellationToken);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project Request"));

            var list = new PagedResult<ProjectRequestResponse>
            {
                Items = _mapper.Map<List<ProjectRequestResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            return list;
        }

        public async Task<PagedResult<ProjectRequestResponse>> SearchProjectRequestAsync(ProjectRequestSearchRequest filter, Guid userCompanyId, Guid partnerId, CancellationToken cancellationToken = default)
        {
            if (filter == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _projectRequestRepository.SearchProjectRequestAsync(filter, userCompanyId, partnerId, cancellationToken);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project Request"));

            var list = new PagedResult<ProjectRequestResponse>
            {
                Items = _mapper.Map<List<ProjectRequestResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            return list;
        }

        public async Task<ProjectRequestResponse> UpdateProjectRequestAsync(Guid id, UpdateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken = default)
        {
            var projectRequest = _mapper.Map<ProjectRequest>(request);
            var response = await _projectRequestRepository.UpdateProjectRequestAsync(id, projectRequest, vendorEmail, cancellationToken);

            //await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            //{
            //    UserId = response.ExecutorCompany.OwnerUser.Id,
            //    Title = $"Project Request {response.Id} Updated",
            //    Body = $"{response.RequesterCompany.OwnerUser.UserName} from {response.RequesterCompany.Name} has updated in project request. Please review and discuss for a smooth cooperation.",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "UPDATE_PROJECT_REQUEST",
            //    Context = null,
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString(),
            //});
            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = request.ExecutorCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Update project request",
                Description = $"User:'{currentUserName}' has updated project request {response.Code} for project {response.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<ProjectRequestResponse>(response);
        }
        private async Task<string?> GetUserName(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().FindAsync(c => c.Id == userId);
            return user.UserName;
        }
    }
}
