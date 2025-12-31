using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.ChatMessage.Requests;
using Fusion.Service.ViewModels.ChatMessage.Responses;


namespace Fusion.Service.Services;

public class ChatService : IChatService
{
    private const int Pending = 0;
    private const int Accepted = 1;
    private const int Rejected = 2;

    private const int Direct = 1;
    private const int Group = 2;

    private const int Member = 0;
    private const int Owner = 1;

    private readonly IUserFriendshipRepository _friendRepo;
    private readonly IChatConversationRepository _convRepo;
    private readonly IChatConversationMemberRepository _memberRepo;
    private readonly IChatMessageRepository _msgRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentService _current;
    private readonly IMapper _mapper;

    public ChatService(IUserFriendshipRepository friendRepo, IChatConversationRepository convRepo, IChatConversationMemberRepository memberRepo,
           IChatMessageRepository msgRepo, IUnitOfWork uow, ICurrentService current, IMapper mapper)
    {
        _friendRepo = friendRepo;
        _convRepo = convRepo;
        _memberRepo = memberRepo;
        _msgRepo = msgRepo;
        _uow = uow;
        _current = current;
        _mapper = mapper;
    }

    // ====== 1) Create-or-Get Direct DM ======
    public async Task<ChatConversationResponse> OpenDirectChatAsync(Guid otherUserId, CancellationToken ct = default)
    {
        var currentUserId = _current.GetUserId();
        if (currentUserId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        if (otherUserId == Guid.Empty || otherUserId == currentUserId)
            throw CustomExceptionFactory.CreateBadRequestError("Invalid other user.");

        // Step 0: friendship must be Accepted
        var pairKey = PairKeyHelper.Build(currentUserId, otherUserId);
        var friendship = await _friendRepo.GetByPairKeyAsync(pairKey, ct);

        var st = friendship?.Status ?? -1;
        if (st != Accepted)
        {
            // Pending/Rejected/Null => 403 không cho mở chat
            throw CustomExceptionFactory.CreateForbiddenError();
        }

        // Step 1: find conversation direct by pairKey
        var existedConv = await _convRepo.GetDirectByPairKeyAsync(pairKey, ct);

        if (existedConv != null)
        {
            // ensure 2 members exist
            var isMeMember = await _memberRepo.IsMemberAsync(existedConv.Id, currentUserId, ct);
            var isOtherMember = await _memberRepo.IsMemberAsync(existedConv.Id, otherUserId, ct);

            if (!isMeMember)
            {
                await _memberRepo.AddAsync(new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = existedConv.Id,
                    UserId = currentUserId,
                    Role = Member,
                    AddedBy = currentUserId,
                    JoinedAt = DateTime.UtcNow
                }, ct);
            }

            if (!isOtherMember)
            {
                await _memberRepo.AddAsync(new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = existedConv.Id,
                    UserId = otherUserId,
                    Role = Member,
                    AddedBy = currentUserId,
                    JoinedAt = DateTime.UtcNow
                }, ct);
            }

            if (!isMeMember || !isOtherMember)
                await _uow.SaveChangesAsync(ct);

            return _mapper.Map<ChatConversationResponse>(existedConv);
        }

        // Step 2: create new conversation + add 2 members
        var conv = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Type = Direct,
            Title = null,
            DirectPairKey = pairKey,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = null
        };

        await _convRepo.AddAsync(conv, ct);

        await _memberRepo.AddRangeAsync(new[]
        {
                new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    UserId = currentUserId,
                    Role = Owner,           // cho A làm owner cũng được
                    AddedBy = currentUserId,
                    JoinedAt = DateTime.UtcNow
                },
                new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    UserId = otherUserId,
                    Role = Member,
                    AddedBy = currentUserId,
                    JoinedAt = DateTime.UtcNow
                }
            }, ct);

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ChatConversationResponse>(conv);
    }

    // ====== 2) Create Group Chat (Option A) ======
    public async Task<ChatConversationResponse> CreateGroupChatAsync(CreateGroupChatRequest request, CancellationToken ct = default)
    {
        var currentUserId = _current.GetUserId();
        if (currentUserId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var memberIds = (request.MemberIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty && x != currentUserId)
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
            throw CustomExceptionFactory.CreateBadRequestError("MemberIds is required.");

        // Rule Option A: A chỉ mời người đã là friend với A (Accepted)
        foreach (var uid in memberIds)
        {
            var pairKey = PairKeyHelper.Build(currentUserId, uid);
            var fr = await _friendRepo.GetByPairKeyAsync(pairKey, ct);
            if ((fr?.Status ?? -1) != Accepted)
                throw CustomExceptionFactory.CreateForbiddenError();
        }

        var conv = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Type = Group,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "New group" : request.Title.Trim(),
            DirectPairKey = null,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = null
        };

        await _convRepo.AddAsync(conv, ct);

        var members = new List<ChatConversationMember>
            {
                new ChatConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    UserId = currentUserId,
                    Role = Owner,
                    AddedBy = currentUserId,
                    JoinedAt = DateTime.UtcNow
                }
            };

        members.AddRange(memberIds.Select(uid => new ChatConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            UserId = uid,
            Role = Member,
            AddedBy = currentUserId,
            JoinedAt = DateTime.UtcNow
        }));

        await _memberRepo.AddRangeAsync(members, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ChatConversationResponse>(conv);
    }

    // ====== FE join SignalR: must be member ======
    public async Task EnsureCanJoinConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        var currentUserId = _current.GetUserId();
        if (currentUserId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        var isMember = await _memberRepo.IsMemberAsync(conversationId, currentUserId, ct);
        if (!isMember)
            throw CustomExceptionFactory.CreateForbiddenError();
    }

    // ====== Send Message (Hub calls this) ======
    public async Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken ct = default)
    {
        var currentUserId = _current.GetUserId();
        if (currentUserId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Current user is not found.");

        if (request.ConversationId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("ConversationId is required.");

        var content = (request.Content ?? "").Trim();
        if (string.IsNullOrWhiteSpace(content))
            throw CustomExceptionFactory.CreateBadRequestError("Content is required.");

        var clientMessageId = (request.ClientMessageId ?? "").Trim();
        if (string.IsNullOrWhiteSpace(clientMessageId))
            throw CustomExceptionFactory.CreateBadRequestError("ClientMessageId is required.");

        // membership check
        var isMember = await _memberRepo.IsMemberAsync(request.ConversationId, currentUserId, ct);
        if (!isMember)
            throw CustomExceptionFactory.CreateForbiddenError();

        var conv = await _convRepo.GetByIdAsync(request.ConversationId, ct);
        if (conv == null)
            throw CustomExceptionFactory.CreateBadRequestError("Conversation not found.");

        // If Direct: check friendship accepted again (anti spam)
        if ((conv.Type ?? 0) == Direct)
        {
            var pairKey = (conv.DirectPairKey ?? "").Trim();
            if (string.IsNullOrWhiteSpace(pairKey))
                throw CustomExceptionFactory.CreateBadRequestError("Direct conversation pair key missing.");

            var fr = await _friendRepo.GetByPairKeyAsync(pairKey, ct);
            if ((fr?.Status ?? -1) != Accepted)
                throw CustomExceptionFactory.CreateForbiddenError();
        }

        // idempotency (reconnect retry)
        var existed = await _msgRepo.GetByClientMessageIdAsync(request.ConversationId, currentUserId, clientMessageId, ct);
        if (existed != null)
            return _mapper.Map<ChatMessageResponse>(existed);

        var now = DateTime.UtcNow;

        var msg = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = currentUserId,
            Content = content,
            ClientMessageId = clientMessageId,
            CreatedAt = now
        };

        await _msgRepo.AddAsync(msg, ct);

        conv.LastMessageAt = now;
        _convRepo.Update(conv);

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ChatMessageResponse>(msg);
    }
}
