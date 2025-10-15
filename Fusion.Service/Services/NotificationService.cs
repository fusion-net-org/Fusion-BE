using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Notifications.Responses;
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

        public async Task CreateAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
        {
            string? linkUrlWeb = null;
            string? linkUrlMobile = null;

            if (!string.IsNullOrWhiteSpace(request.LinkKey))
            {
                (linkUrlWeb, linkUrlMobile) = NotificationRouteMap.Resolve(request.LinkKey, request.Id);
            }

            var notification = _mapper.Map<Notification>(request);

            await _notificationRepository.CreateAsync(notification, linkUrlWeb, linkUrlMobile, cancellationToken);

            await _fcmService.SendToUserAsync(request.UserId, request.Title, request.Body, linkUrlWeb, linkUrlMobile, cancellationToken);

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
