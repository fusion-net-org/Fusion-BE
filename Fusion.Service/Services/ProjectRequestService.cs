using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Projects.Requests;
using Fusion.Service.ViewModels.Projects.Responses;
using static Google.Apis.Requests.BatchRequest;
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


            var emailBody = $@"
               <h3>Dear {result.RequesterCompany?.Name},</h3>
                <p>Your project request <strong>{result?.Name}</strong> 
                has been <b style='color:green;'>ACCEPTED</b> by 
                {result.ExecutorCompany?.Name}.</p>
                <p>Start Date: {result.StartDate:dd/MM/yyyy}<br/>
                End Date: {result.EndDate:dd/MM/yyyy}</p>
                <p>You can now proceed with the project coordination.</p>
                <hr/>
                <small>Fusion System Notification</small>"";
            ";

            await _mailService.SendEmailAsync(new ViewModels.Companies.Email.MailRequest()
            {
                Subject = $"Your project request \"{result?.Name}\" has been accepted!",
                Body = emailBody,
                ToEmail = result.ExecutorCompany.OwnerUser.Email
            });

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = result.ExecutorCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Accept project request",
                Description = $"User: '{currentUserName}' has accepted project request {result.Code} for project {result.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);

            // Notify requester 
            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = result.RequesterCompany.OwnerUser.Id,
                Title = "Project request accepted",
                Body = $"{result.ExecutorCompany.OwnerUser.UserName} from {result.ExecutorCompany.Name} has accepted your project request \"{result.Name}\".",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = result.RequesterCompanyId,
                Event = "PROJECT_REQUEST_ACCEPT",
                Context = result.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);

            // Notify executor 
            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = result.ExecutorCompany.OwnerUser.Id,
                Title = "You accepted a project request",
                Body = $"You have accepted the project request \"{result.Name}\" from {result.RequesterCompany.Name}.",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = result.ExecutorCompanyId,
                Event = "PROJECT_REQUEST_ACCEPT_CONFIRM",
                Context = result.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);

            return _mapper.Map<ProjectRequestResponse>(result);
        }

        public async Task<ProjectRequestResponse> AddProjectRequestAsync(CreateProjectRequestRequest request, string vendorEmail, CancellationToken cancellationToken)
        {
            var projectRequest = _mapper.Map<ProjectRequest>(request);
            var code = ProjectCodeUtil.GenerateProjectRequestCode();
            var response = await _projectRequestRepository.AddProjectRequestAsync(projectRequest, vendorEmail, code, cancellationToken);

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
                CompanyId = request.RequesterCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Create project request",
                Description = $"User:'{currentUserName}' has created project request {response.Code} for project {response.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);

            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = response.ExecutorCompany.OwnerUser.Id,
                Title = "New project request received",
                Body = $"{response.RequesterCompany.OwnerUser.UserName} from {response.RequesterCompany.Name} has requested your company to collaborate on project \"{response.Name}\".",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = response.ExecutorCompanyId,
                Event = "CREATE_PROJECT_REQUEST",
                Context = response.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);

            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = response.RequesterCompany.OwnerUser.Id,
                Title = "Project request sent successfully",
                Body = $"You have sent a collaboration project request \"{response.Name}\" to company {response.ExecutorCompany.Name}.",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = response.RequesterCompanyId,
                Event = "CREATE_PROJECT_REQUEST_CONFIRM",
                Context = response.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);



            return _mapper.Map<ProjectRequestResponse>(response);
        }

        public async Task<bool> DeleteProjectRequestAsync(Guid id,string reason, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentService.GetUserId();

            var result = await _projectRequestRepository.DeleteProjectRequestAsync(id,reason, currentUserId, cancellationToken);

            var projectRequest = await _projectRequestRepository.GetProjectRequestByIdAsync(id);
            var currentUserName = await GetUserName(currentUserId);

            var log = new CompanyActivityLog
            {
                CompanyId = projectRequest?.RequesterCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Delete project request",
                Description = $"User:'{currentUserName}' has deleted project request {projectRequest?.Code} for project {projectRequest?.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);

            var requesterUserId = projectRequest.RequesterCompany.OwnerUser.Id;
            var executorUserId = projectRequest.ExecutorCompany.OwnerUser.Id;
            var deletedByRequester = requesterUserId == currentUserId;


            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = requesterUserId,
                Title = "Project request deleted",
                Body = deletedByRequester
                    ? $"You have deleted the project request \"{projectRequest.Name}\"."
                    : $"{projectRequest.ExecutorCompany.OwnerUser.UserName} has deleted your project request \"{projectRequest.Name}\".",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = projectRequest.RequesterCompanyId,
                Event = "DELETE_PROJECT_REQUEST",
                Context = projectRequest.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);

            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = executorUserId,
                Title = "Project request deleted",
                Body = deletedByRequester
                    ? $"{projectRequest.RequesterCompany.OwnerUser.UserName} has deleted the project request \"{projectRequest.Name}\"."
                    : $"You have deleted the project request \"{projectRequest.Name}\".",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = projectRequest.ExecutorCompanyId,
                Event = "DELETE_PROJECT_REQUEST",
                Context = projectRequest.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);

            return result;
        }
        public async Task<bool> RestoreProjectRequestAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentService.GetUserId();

            var result = await _projectRequestRepository.RestoreProjectRequestAsync(id, currentUserId, cancellationToken);
            if (!result) return false;

            var projectRequest = await _projectRequestRepository.GetProjectRequestByIdAsync(id);
            if (projectRequest == null) return false;

            var currentUserName = await GetUserName(currentUserId);

            var log = new CompanyActivityLog
            {
                CompanyId = projectRequest?.RequesterCompanyId ?? Guid.Empty,
                ActorUserId = currentUserId,
                Title = "Restore project request",
                Description = $"User '{currentUserName}' has restored project request '{projectRequest.Code}' - {projectRequest.Name}"
            };
            await _logService.CreateLog(log, cancellationToken);

            var requesterUserId = projectRequest.RequesterCompany?.OwnerUser?.Id ?? Guid.Empty;
            var executorUserId = projectRequest.ExecutorCompany?.OwnerUser?.Id ?? Guid.Empty;

            if (requesterUserId != Guid.Empty)
            {
                await _notificationService.CreateNotificationAsync(new SendNotificationRequest
                {
                    UserId = requesterUserId,
                    Title = "Project request restored",
                    Body = "You have restored this project request.",
                    LinkKey = "PROJECT_REQUEST_PAGE",
                    IdLink = projectRequest.RequesterCompanyId,
                    Event = "RESTORE_PROJECT_REQUEST",
                    Context = projectRequest.Name,
                    NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString()
                }, cancellationToken);
            }

            if (executorUserId != Guid.Empty)
            {
                await _notificationService.CreateNotificationAsync(new SendNotificationRequest
                {
                    UserId = executorUserId,
                    Title = "Project request restored",
                    Body = $"{currentUserName} has restored the project request '{projectRequest.Name}'.",
                    LinkKey = "PROJECT_REQUEST_PAGE",
                    IdLink = projectRequest.ExecutorCompanyId,
                    Event = "RESTORE_PROJECT_REQUEST",
                    Context = projectRequest.Name,
                    NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString()
                }, cancellationToken);
            }

            return true;
        }

        public async Task<ProjectRequestResponse?> GetProjectRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.GetProjectRequestByIdAsync(id, cancellationToken);
            return _mapper?.Map<ProjectRequestResponse?>(result);
        }

        public async Task<ProjectRequestRejectResponse> RejectProjectRequestAsync(Guid requestId, string executorEmail, string reason, CancellationToken cancellationToken = default)
        {
            var result = await _projectRequestRepository.RejectProjectRequestAsync(requestId, executorEmail, reason, cancellationToken);

            if (result == null)
                throw CustomExceptionFactory.CreateBadRequestError("Reject project request failed");

            var projectRequest = await _projectRequestRepository.GetProjectRequestByIdAsync(requestId);

            var emailBody = $@"
                <h3>Dear {result.RequesterCompany?.Name},</h3>
                <p>Your project request <strong>{result?.Name}</strong> 
                has been <b style='color:red;'>REJECTED</b> by 
                {result.ExecutorCompany?.Name}.</p>
                <p><b>Reason:</b> {result.ReasonReject ?? "No specific reason provided."}</p>
                <p>Please review your project details and resubmit if needed.</p>
                <hr/>
                <small>Fusion System Notification</small>"";
            ";

            await _mailService.SendEmailAsync(new ViewModels.Companies.Email.MailRequest()
            {
                Subject = $"Your project request \"{result?.Name}\" has been rejected.",
                Body = emailBody,
                ToEmail = result.ExecutorCompany.OwnerUser.Email
            });


            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = projectRequest?.ExecutorCompanyId ?? Guid.Empty,
                ActorUserId = _currentService.GetUserId(),
                Title = "Reject project request",
                Description = $"User:'{currentUserName}' has rejected project request {projectRequest?.Code} for project {projectRequest?.Name}",
            };
            await _logService.CreateLog(log, cancellationToken);

            var requesterUserId = projectRequest.RequesterCompany.OwnerUser.Id;
            var executorUserId = projectRequest.ExecutorCompany.OwnerUser.Id;

            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = requesterUserId,
                Title = "Project request rejected",
                Body = $"{projectRequest.ExecutorCompany.OwnerUser.UserName} from {projectRequest.ExecutorCompany.Name} has rejected your project request \"{projectRequest.Name}\". Reason: {reason}.",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = projectRequest.RequesterCompanyId,
                Event = "PROJECT_REQUEST_REJECT",
                Context = projectRequest.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);

            await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            {
                UserId = executorUserId,
                Title = "You rejected a project request",
                Body = $"You have rejected the project request \"{projectRequest.Name}\" from {projectRequest.RequesterCompany.Name}.",
                LinkKey = "PROJECT_REQUEST_PAGE",
                IdLink = projectRequest.ExecutorCompanyId,
                Event = "PROJECT_REQUEST_REJECT",
                Context = projectRequest.Name,
                NotificationType = NotificationTypeEnum.PROJECT_REQUEST.ToString(),
            }, cancellationToken);


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
                CompanyId = request.RequesterCompanyId ?? Guid.Empty,
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
