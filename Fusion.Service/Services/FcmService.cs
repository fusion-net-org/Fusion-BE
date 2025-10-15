using FirebaseAdmin.Messaging;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
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

        public async Task SendToUserAsync(Guid userId, string title, string? body, string? linkUrlWeb = null,
            string? linkUrlMobile = null, CancellationToken cancellationToken = default)
        {

            var tokens = await _userDeviceRepository.GetTokensByUserIdAsync(userId);

            if (!tokens.Any())
                return;

            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    { "linkUrlWeb", linkUrlWeb ?? "" },
                    { "linkUrlMobile", linkUrlMobile ?? "" },
                    { "type", "BUSINESS" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }
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
