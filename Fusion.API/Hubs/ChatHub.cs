using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Notifications.Requests;
using Microsoft.AspNetCore.SignalR;

namespace Fusion.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public ChatHub(IUserRepository userRepository, INotificationService notificationService)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task JoinGroup(string groupKey)
        {
            Console.WriteLine($"User {Context.UserIdentifier} joined {groupKey}");
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"GROUP_{groupKey}"
            );
        }

        public async Task LeaveGroup(string groupKey)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"GROUP_{groupKey}"
            );
        }

        public async Task SendGroupMessage(string groupKey, string message, List<Guid>? mentionUserIds)
        {
            var fromUserId = Guid.Parse(Context.UserIdentifier);


            Console.WriteLine($"User {Context.UserIdentifier} joined {groupKey}");
            var sender = await _userRepository.GetUserByIdAsync(fromUserId);

            if (sender == null)
            {
                throw CustomExceptionFactory.CreateNotFoundError("User is not existed");
            }

            //var msg = await _chatService .SaveGroupMessageAsync(groupKey, fromUserId, message);

            //Sẽ send cái object của ChatMessage(Cải tiến)
            await Clients.Group($"GROUP_{groupKey}")
                .SendAsync("ReceiveGroupMessage", message);

            if (mentionUserIds?.Any() == true)
            {
                foreach (var userId in mentionUserIds)
                {
                    if (userId == fromUserId) continue;

                    //await Clients.User(userId.ToString())
                    //    .SendAsync("ReceiveMentionNotification", new
                    //    {
                    //        GroupKey = groupKey,
                    //        MessageId = msg.Id,
                    //        FromUserId = fromUserId,
                    //        Message = message
                    //    });

                    await _notificationService.CreateNotificationAsync(new SendNotificationRequest
                    {
                        UserId = userId,
                        Title = "You were mentioned in a Group chat",
                        Body = $"{sender.UserName} mentioned you in group {groupKey}.",
                        LinkKey = "GROUP_CHAT",
                        IdLink = null,
                        Event = "GroupChatMention",
                        NotificationType = NotificationTypeEnum.MENTION.ToString()
                    });
                }
            }
        }


        public async Task SendPrivateMessage(Guid toUserId, string message)
        {
            var fromUserId = Guid.Parse(Context.UserIdentifier);

            Console.WriteLine($"User mentioned you in group joined.");

            var sender = await _userRepository.GetUserByIdAsync(fromUserId);

            if (sender == null)
            {
                throw CustomExceptionFactory.CreateNotFoundError("User is not existed");
            }

            //var msg = await _chatService.SavePrivateMessageAsync(fromUserId, toUserId, message);

            //Sẽ send cái object của ChatMessage(Cải tiến)

            await Clients.User(toUserId.ToString())
                .SendAsync("ReceivePrivateMessage", message);

            await Clients.User(fromUserId.ToString())
                .SendAsync("ReceivePrivateMessage", message);
        }

    }
}
