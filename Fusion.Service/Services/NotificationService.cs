using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
using Google.Api.Gax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class NotificationService: INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFcmService _fcmService;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IFcmService fcmService, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _fcmService = fcmService;
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
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT, $"Invalid type: {request.NotificationType}");
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
            }, cancellationToken);


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
                Type = notificationReceive.LinkUrlWeb,
            }, cancellationToken);


        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var result = await _notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);

            return _mapper.Map<IEnumerable<NotificationResponse>>(result);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            await _notificationRepository.MarkAsReadAsync(userId, notificationId, cancellationToken);
        }
    }
}
