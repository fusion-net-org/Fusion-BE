using FirebaseAdmin.Messaging;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class FcmService : IFcmService
    {
        private readonly IUserDeviceRepository _userDeviceRepository;

        public FcmService(IUserDeviceRepository userDeviceRepository)
        {
            _userDeviceRepository = userDeviceRepository;
        }

        public async Task SendToUserAsync(FCMNotificationRequest request, CancellationToken cancellationToken = default)
        {

            var tokens = await _userDeviceRepository.GetTokensByUserIdAsync(request.UserId);

            if (!tokens.Any())
                return;

            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = request.Title,
                    Body = request.Body
                },
                Data = new Dictionary<string, string>
                    {
                        {"NotificationId", request.NotificationId.ToString() },
                        { "linkUrlWeb", request.LinkUrlWeb ?? "" },
                        { "linkUrlMobile", request.LinkUrlMobile ?? "" },
                        { "type", request.Type },
                        { "timestamp", DateTime.UtcNow.ToString("O") },
                    },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "default",
                        Sound = "default",
                    }
                },

            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);

            for (int i = 0; i < response.Responses.Count; i++)
            {
                var result = response.Responses[i];
                if (!result.IsSuccess)
                {
                    var error = result.Exception;
                    if (error is FirebaseMessagingException fcmEx &&
                        (fcmEx.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                         fcmEx.MessagingErrorCode == MessagingErrorCode.InvalidArgument))
                    {
                        await _userDeviceRepository.DeactivateTokenAsync(tokens[i], cancellationToken);
                    }
                }
            }

            Console.WriteLine($"📨 Sent: {response.SuccessCount} success, {response.FailureCount} failed");

        }
    }
}
