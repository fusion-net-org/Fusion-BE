using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.Notifications.Requests;
using Microsoft.AspNetCore.SignalR;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Fusion.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IChatService _chatService;

        public ChatHub(IUserRepository userRepository, INotificationService notificationService, IChatService chatService)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
            _chatService = chatService;
        }

        private async Task<User> GetCurrentUserAsync()
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId))
                throw CustomExceptionFactory.CreateUnauthorizedError();

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User is not existed");

            return user;
        }

        public async Task JoinGroup(Guid conversationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"GROUP_{conversationId}"
            );
        }

        public async Task LeaveGroup(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"GROUP_{conversationId}"
            );
        }

        public async Task SendGroupMessage(Guid conversationId, string clientMessageId, string message, List<Guid>? mentionUserIds)
        {
            var sender = await GetCurrentUserAsync();

            if (sender == null)
            {
                throw CustomExceptionFactory.CreateNotFoundError("User is not existed");
            }

            var msg = await _chatService.SendMessageAsync(new SendMessageRequest
            {
                ClientMessageId = clientMessageId,
                Content = message,
                ConversationId = conversationId,
            });

            await Clients.Group($"GROUP_{conversationId}")
                .SendAsync("ReceiveGroupMessage", msg);

            if (mentionUserIds?.Any() == true)
            {
                foreach (var userId in mentionUserIds)
                {
                    if (userId == sender.Id) continue;

                    await _notificationService.CreateNotificationAsync(new SendNotificationRequest
                    {
                        UserId = userId,
                        Title = "You were mentioned in a Group chat",
                        Body = $"{sender.UserName} mentioned you in group {conversationId}.",
                        LinkKey = "GROUP_CHAT",
                        IdLink = null,
                        Event = "GroupChatMention",
                        NotificationType = NotificationTypeEnum.MENTION.ToString()
                    });
                }
            }
        }


        public async Task SendPrivateMessage(Guid toUserId, Guid conversationId, string clientMessageId, string message)
        {
            var sender = await GetCurrentUserAsync();
            if (sender == null)
                throw CustomExceptionFactory.CreateNotFoundError("User is not existed");

            var msg = await _chatService.SendMessageAsync(new SendMessageRequest
            {
                ClientMessageId = clientMessageId,
                Content = message,
                ConversationId = conversationId,
            });
       
            await Clients.User(toUserId.ToString())
                .SendAsync("ReceivePrivateMessage", msg);

            await Clients.User(sender.Id.ToString())
                .SendAsync("ReceivePrivateMessage", msg);

            await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            {
                UserId = toUserId,
                Title = "You were mentioned in a Private chat",
                Body = $"{sender.UserName} mentioned you in chat {conversationId}.",
                LinkKey = "PRIVATE_CHAT",
                IdLink = null,
                Event = "PrivateChat",
                NotificationType = NotificationTypeEnum.MENTION.ToString()
            });
        }

    }
}
