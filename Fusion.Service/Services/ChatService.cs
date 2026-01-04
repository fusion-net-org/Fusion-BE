
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Chat;
using Fusion.Repository.Common;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Chat;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.ChatMessage.Responses;



namespace Fusion.Service.Services;

public class ChatService : IChatService
{
    private readonly IUserFriendshipRepository _friendRepo;
    private readonly IChatConversationRepository _convRepo;
    private readonly IChatConversationMemberRepository _memberRepo;
    private readonly IChatMessageRepository _msgRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentService _current;

    public ChatService(
       IUserFriendshipRepository friendRepo,
       IChatConversationRepository convRepo,
       IChatConversationMemberRepository memberRepo,
       IChatMessageRepository msgRepo,
       IUnitOfWork uow,
       ICurrentService current)
    {
        _friendRepo = friendRepo;
        _convRepo = convRepo;
        _memberRepo = memberRepo;
        _msgRepo = msgRepo;
        _uow = uow;
        _current = current;
    }

    // called when Accept friend
    public async Task EnsureDirectConversationExistsAsync(Guid userA, Guid userB, CancellationToken ct = default)
    {
        if (userA == Guid.Empty || userB == Guid.Empty || userA == userB) return;

        var chatKey = ChatKeyHelper.BuildDirect(userA, userB);

        // 1) new key
        var conv = await _convRepo.GetByChatKeyAsync(chatKey, ct);

        // 2) backward compatible old data: stored raw pairKey
        if (conv == null)
        {
            var oldPairKey = PairKeyHelper.Build(userA, userB);
            conv = await _convRepo.GetByChatKeyAsync(oldPairKey, ct);

            // migrate value only (no schema change)
            if (conv != null && conv.DirectPairKey != chatKey)
            {
                conv.DirectPairKey = chatKey;
                _convRepo.Update(conv);
                await _uow.SaveChangesAsync(ct);
            }
        }

        if (conv != null)
        {
            // ensure both members exist
            var aMem = await _memberRepo.IsMemberAsync(conv.Id, userA, ct);
            var bMem = await _memberRepo.IsMemberAsync(conv.Id, userB, ct);

            if (!aMem)
                await _memberRepo.AddAsync(new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    UserId = userA,
                    Role = ConversationRole.Member,
                    AddedBy = userA,
                    JoinedAt = DateTime.UtcNow
                }, ct);

            if (!bMem)
                await _memberRepo.AddAsync(new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    UserId = userB,
                    Role = ConversationRole.Member,
                    AddedBy = userA,
                    JoinedAt = DateTime.UtcNow
                }, ct);

            if (!aMem || !bMem) await _uow.SaveChangesAsync(ct);
            return;
        }

        // create new direct conversation
        var now = DateTime.UtcNow;
        var newConv = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.Direct,
            Title = null,
            DirectPairKey = chatKey, //  Dr_...
            CreatedBy = userA,
            CreatedAt = now,
            LastMessageAt = null
        };

        await _convRepo.AddAsync(newConv, ct);
        await _memberRepo.AddRangeAsync(new[]
        {
            new ChatConversationMember
            {
                Id = Guid.NewGuid(),
                ConversationId = newConv.Id,
                UserId = userA,
                Role = ConversationRole.Owner,
                AddedBy = userA,
                JoinedAt = now
            },
            new ChatConversationMember
            {
                Id = Guid.NewGuid(),
                ConversationId = newConv.Id,
                UserId = userB,
                Role = ConversationRole.Member,
                AddedBy = userA,
                JoinedAt = now
            }
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task<ChatConversationResponse> OpenDirectChatAsync(Guid otherUserId, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");
        if (otherUserId == Guid.Empty || otherUserId == me) throw CustomExceptionFactory.CreateBadRequestError("Invalid other user.");

        // friendship must be accepted
        var pairKey = PairKeyHelper.Build(me, otherUserId);
        var fr = await _friendRepo.GetByPairKeyAsync(pairKey, ct);
        if ((fr?.Status ?? -1) != FriendshipStatus.Accepted)
            throw CustomExceptionFactory.CreateForbiddenError();

        await EnsureDirectConversationExistsAsync(me, otherUserId, ct);

        var chatKey = ChatKeyHelper.BuildDirect(me, otherUserId);
        var conv = await _convRepo.GetByChatKeyAsync(chatKey, ct);
        if (conv == null) throw CustomExceptionFactory.CreateBadRequestError("Conversation not found.");

        return new ChatConversationResponse
        {
            Id = conv.Id,
            Type = conv.Type ?? 0,
            Title = conv.Title,
            DirectPairKey = conv.DirectPairKey,
            LastMessageAt = conv.LastMessageAt
        };
    }

    public async Task<ChatConversationResponse> CreateGroupChatAsync(CreateGroupChatRequest request, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var memberIds = (request.MemberIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty && x != me)
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
            throw CustomExceptionFactory.CreateBadRequestError("MemberIds is required.");

        // group create: creator only can add friends accepted
        foreach (var uid in memberIds)
        {
            var pk = PairKeyHelper.Build(me, uid);
            var fr = await _friendRepo.GetByPairKeyAsync(pk, ct);
            if ((fr?.Status ?? -1) != FriendshipStatus.Accepted)
                throw CustomExceptionFactory.CreateForbiddenError();
        }

        var now = DateTime.UtcNow;
        var convId = Guid.NewGuid();

        var conv = new ChatConversation
        {
            Id = convId,
            Type = ConversationType.Group,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "New group" : request.Title.Trim(),
            DirectPairKey = ChatKeyHelper.BuildGroup(convId), //  Gr_...
            CreatedBy = me,
            CreatedAt = now,
            LastMessageAt = null
        };

        await _convRepo.AddAsync(conv, ct);

        var members = new List<ChatConversationMember>
        {
            new ChatConversationMember
            {
                Id = Guid.NewGuid(),
                ConversationId = convId,
                UserId = me,
                Role = ConversationRole.Owner,
                AddedBy = me,
                JoinedAt = now
            }
        };

        members.AddRange(memberIds.Select(uid => new ChatConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = convId,
            UserId = uid,
            Role = ConversationRole.Member,
            AddedBy = me,
            JoinedAt = now
        }));

        await _memberRepo.AddRangeAsync(members, ct);
        await _uow.SaveChangesAsync(ct);

        return new ChatConversationResponse
        {
            Id = conv.Id,
            Type = conv.Type ?? 0,
            Title = conv.Title,
            DirectPairKey = conv.DirectPairKey,
            LastMessageAt = conv.LastMessageAt
        };
    }

    // any group member can invite their accepted friends
    public async Task InviteMembersToGroupAsync(Guid conversationId, InviteGroupMembersRequest request, CancellationToken ct = default)
    {
        var inviter = _current.GetUserId();
        if (inviter == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var conv = await _convRepo.GetByIdAsync(conversationId, ct);
        if (conv == null) throw CustomExceptionFactory.CreateBadRequestError("Conversation not found.");
        if ((conv.Type ?? 0) != ConversationType.Group)
            throw CustomExceptionFactory.CreateBadRequestError("This conversation is not a group.");

        // inviter must be member
        var inviterIsMember = await _memberRepo.IsMemberAsync(conversationId, inviter, ct);
        if (!inviterIsMember) throw CustomExceptionFactory.CreateForbiddenError();

        var inviteIds = (request.MemberIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty && x != inviter)
            .Distinct()
            .ToList();

        if (inviteIds.Count == 0) return;

        var now = DateTime.UtcNow;

        foreach (var uid in inviteIds)
        {
            // must be friends accepted with inviter
            var pk = PairKeyHelper.Build(inviter, uid);
            var fr = await _friendRepo.GetByPairKeyAsync(pk, ct);
            if ((fr?.Status ?? -1) != FriendshipStatus.Accepted)
                throw CustomExceptionFactory.CreateForbiddenError();

            // skip if already member
            if (await _memberRepo.IsMemberAsync(conversationId, uid, ct)) continue;

            await _memberRepo.AddAsync(new ChatConversationMember
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = uid,
                Role = ConversationRole.Member,
                AddedBy = inviter,
                JoinedAt = now
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
    public async Task EnsureCanJoinConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var isMember = await _memberRepo.IsMemberAsync(conversationId, me, ct);
        if (!isMember) throw CustomExceptionFactory.CreateForbiddenError();
    }

    public async Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        if (request.ConversationId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("ConversationId is required.");

        var content = (request.Content ?? "").Trim();
        if (string.IsNullOrWhiteSpace(content))
            throw CustomExceptionFactory.CreateBadRequestError("Content is required.");

        var clientMessageId = (request.ClientMessageId ?? "").Trim();
        if (string.IsNullOrWhiteSpace(clientMessageId))
            throw CustomExceptionFactory.CreateBadRequestError("ClientMessageId is required.");

        // membership check
        var isMember = await _memberRepo.IsMemberAsync(request.ConversationId, me, ct);
        if (!isMember) throw CustomExceptionFactory.CreateForbiddenError();

        var conv = await _convRepo.GetByIdAsync(request.ConversationId, ct);
        if (conv == null) throw CustomExceptionFactory.CreateBadRequestError("Conversation not found.");

        // direct anti-spam
        if ((conv.Type ?? 0) == ConversationType.Direct)
        {
            if (!ChatKeyHelper.TryExtractFriendPairKey(conv.DirectPairKey, out var pairKey))
                throw CustomExceptionFactory.CreateBadRequestError("Direct chat key missing.");

            var fr = await _friendRepo.GetByPairKeyAsync(pairKey, ct);
            if ((fr?.Status ?? -1) != FriendshipStatus.Accepted)
                throw CustomExceptionFactory.CreateForbiddenError();
        }

        // idempotency
        var existed = await _msgRepo.GetByClientMessageIdAsync(request.ConversationId, me, clientMessageId, ct);
        if (existed != null)
        {
            return new ChatMessageResponse
            {
                Id = existed.Id,
                ConversationId = existed.ConversationId ?? Guid.Empty,
                SenderId = existed.SenderId ?? Guid.Empty,
                Content = existed.Content,
                ClientMessageId = existed.ClientMessageId,
                CreatedAt = existed.CreatedAt
            };
        }

        var now = DateTime.UtcNow;

        var msg = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = me,
            Content = content,
            ClientMessageId = clientMessageId,
            CreatedAt = now
        };

        await _msgRepo.AddAsync(msg, ct);

        conv.LastMessageAt = now;
        _convRepo.Update(conv);

        await _uow.SaveChangesAsync(ct);

        return new ChatMessageResponse
        {
            Id = msg.Id,
            ConversationId = msg.ConversationId ?? Guid.Empty,
            SenderId = msg.SenderId ?? Guid.Empty,
            Content = msg.Content,
            ClientMessageId = msg.ClientMessageId,
            CreatedAt = msg.CreatedAt
        };
    }

    // ===== APIs for FE =====
    public async Task<PagedResult<ChatConversationListItemVm>> GetMyConversationsPagedAsync(ChatConversationPagedRequest request, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");
        return await _convRepo.GetMyConversationsPagedAsync(me, request, ct);
    }

    public async Task<ChatConversationDetailVm> GetConversationByIdAsync(Guid conversationId, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var isMember = await _memberRepo.IsMemberAsync(conversationId, me, ct);
        if (!isMember) throw CustomExceptionFactory.CreateForbiddenError();

        var detail = await _convRepo.GetConversationDetailAsync(conversationId, ct);
        if (detail == null) throw CustomExceptionFactory.CreateBadRequestError("Conversation not found.");

        return detail;
    }

    public async Task<PagedResult<ChatMessageVm>> GetMessagesPagedAsync(Guid conversationId, ChatMessagePagedRequest request, CancellationToken ct = default)
    {
        var me = _current.GetUserId();
        if (me == Guid.Empty) throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var isMember = await _memberRepo.IsMemberAsync(conversationId, me, ct);
        if (!isMember) throw CustomExceptionFactory.CreateForbiddenError();

        request.ConversationId = conversationId;
        return await _msgRepo.GetMessagesPagedAsync(conversationId, request, ct);
    }
}
