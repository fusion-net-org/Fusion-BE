using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using Fusion.Service.ViewModels.Projects.Responses;
using Google.Api.Gax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fusion.Service.Services
{
    public class NotificationService: INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFcmService _fcmService;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IFcmService fcmService, ITaskRepository taskRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _fcmService = fcmService;
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task CreateNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
        {
            string? linkUrlWeb = null;
            string? linkUrlMobile = null;

            if (!string.IsNullOrWhiteSpace(request.LinkKey))
            {
                (linkUrlWeb, linkUrlMobile) = NotificationRouteMap.Resolve(request.LinkKey, request.IdLink);
            }

            if (!Enum.TryParse<NotificationTypeEnum>(request.NotificationType, true, out var typeEnum))
            {
                throw CustomExceptionFactory.CreateBadRequestError($"Invalid type: {request.NotificationType}");
            }

            var notification = _mapper.Map<Notification>(request);

            var notificationReceive = await _notificationRepository.CreateAsync(notification, request.NotificationType, linkUrlWeb, linkUrlMobile, cancellationToken);

            await _fcmService.SendToUserAsync(new FCMNotificationRequest 
            { 
                NotificationId = notificationReceive.Id, 
                Body = request.Body,
                Title = request.Title,
                LinkUrlMobile = linkUrlMobile,
                LinkUrlWeb = linkUrlWeb,
                Type = request.NotificationType,
                UserId = request.UserId,
            }, request.NotificationType, cancellationToken);


        }

        public async Task SendNotificationToTaskMembersAsync(Guid taskId, Guid userId, SendTaskCommentNotificationRequest request, CancellationToken cancellationToken = default)
        {
            var memberIds = await _taskRepository.GetMemberIdByTaskId(taskId, cancellationToken);

            if (!memberIds.Any())
                throw CustomExceptionFactory.CreateNotFoundError("Task member is not existed");

            if (!memberIds.Contains(userId))
                throw CustomExceptionFactory.CreateBadRequestError("User does not belong to this task");

            foreach (var memberId in memberIds)
            {
                if (userId == memberId)
                    continue;

                var notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                    UserId = memberId,
                    IsRead = false,
                    IsDeleted = false,
                    CreateAt = DateTime.UtcNow.AddHours(7)
                };

                var savedNotification = await _notificationRepository.CreateTaskCommentNotificationAsync(notification, cancellationToken);

                if(savedNotification == null)
                    throw CustomExceptionFactory.CreateBadRequestError("Error when send notification to member");

                await _fcmService.SendToUserAsync(new FCMNotificationRequest
                {
                    NotificationId = savedNotification.Id,
                    Body = request.Body,
                    Title = request.Title,
                    LinkUrlMobile = null,
                    LinkUrlWeb = null,
                    Type = savedNotification.NotificationType,
                    UserId = memberId,
                }, savedNotification.NotificationType, cancellationToken);
            }
        }
        public async Task SendAllNotificationAsync(SendAllNotificationRequest request, CancellationToken cancellationToken = default)
        {

            var notification = _mapper.Map<Notification>(request);

            var notificationReceive = await _notificationRepository.CreateAdminNotificationAsync(notification,cancellationToken);

            await _fcmService.SendToAllAsync(new FCMNotificationRequest
            {
                NotificationId = notificationReceive.Id,
                Body = request.Body,
                Title = request.Title,
                LinkUrlMobile = notificationReceive.LinkUrlMobile,
                LinkUrlWeb = notificationReceive.LinkUrlWeb,
                Type = notificationReceive.NotificationType,
            }, cancellationToken);


        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var result = await _notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);

            return _mapper.Map<IEnumerable<NotificationResponse>>(result);
        }

        public async Task<PagedResult<NotificationResponse>> GetAdminNotificationsAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default)
        {
            var result = await _notificationRepository.GetAdminNotificationsAsync(pagedRequest, cancellationToken);

            var list = new PagedResult<NotificationResponse>
            {
                Items = _mapper.Map<List<NotificationResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            return list;
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            await _notificationRepository.MarkAsReadAsync(userId, notificationId, cancellationToken);
        }

        public async Task DeleteNotificationAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            await _notificationRepository.DeleteNotificationAsync(userId,notificationId, cancellationToken);
        }

        public async Task DeleteAllNotificationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _notificationRepository.DeleteAllNotificationByUserIdAsync(userId,cancellationToken);
        }

        public async Task DeleteAdminNotificationAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _notificationRepository.DeleteAdminNotificationAsync(userId, cancellationToken);
        }

        public async Task ToggleNotificationByTypeAsync(Guid userId, ToggleNotificationRequest? request, CancellationToken cancellationToken = default)
        {
            await _notificationRepository.ToggleNotificationByTypeAsync(userId, request.type.Value, request.isEnable, cancellationToken);
        }
    }
}
