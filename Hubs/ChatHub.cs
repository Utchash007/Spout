using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IUnitOfWork _unitOfWork;

    // Static dictionary to track which connectionIds belong to which profileId
    // In production, use Redis or a distributed cache instead
    private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
    private static readonly object _lock = new();

    public ChatHub(IMessageService messageService, IUnitOfWork unitOfWork)
    {
        _messageService = messageService;
        _unitOfWork = unitOfWork;
    }

    // ─── Connection Lifecycle ───

    public override async Task OnConnectedAsync()
    {
        var profileId = await GetProfileId();
        if (profileId == null) return;

        // Track connection
        lock (_lock)
        {
            if (!_userConnections.ContainsKey(profileId))
                _userConnections[profileId] = new HashSet<string>();
            _userConnections[profileId].Add(Context.ConnectionId);
        }

        // Join a group named after the user's profileId for targeted messaging
        await Groups.AddToGroupAsync(Context.ConnectionId, profileId);

        // Notify other users that this user is online
        await Clients.Others.SendAsync("UserOnline", profileId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var profileId = await GetProfileId();
        if (profileId != null)
        {
            lock (_lock)
            {
                if (_userConnections.ContainsKey(profileId))
                {
                    _userConnections[profileId].Remove(Context.ConnectionId);
                    if (_userConnections[profileId].Count == 0)
                        _userConnections.Remove(profileId);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, profileId);

            // Only broadcast offline if user has NO remaining connections
            bool isStillOnline;
            lock (_lock)
            {
                isStillOnline = _userConnections.ContainsKey(profileId);
            }
            if (!isStillOnline)
            {
                await Clients.Others.SendAsync("UserOffline", profileId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ─── Client-Callable Methods ───

    /// <summary>
    /// Called by the JS client when user sends a message.
    /// Saves to DB, then broadcasts to the recipient's group.
    /// </summary>
    public async Task SendMessage(string conversationId, string content)
    {
        var senderProfileId = await GetProfileId();
        if (senderProfileId == null || string.IsNullOrWhiteSpace(content)) return;

        // Save message to database via the existing service
        var message = await _messageService.SendMessage(conversationId, senderProfileId, content);

        // Load sender profile for display info
        var sender = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.Id == senderProfileId);

        var messagePayload = new
        {
            id = message.Id,
            conversationId = message.ConversationId,
            senderProfileId = message.SenderProfileId,
            senderFirstName = sender?.FirstName ?? "",
            senderLastName = sender?.LastName ?? "",
            content = message.Content,
            createdAt = message.CreatedAt.ToString("o"),    // ISO 8601
            isRead = message.IsRead
        };

        // Get all participants of this conversation
        var participants = await _unitOfWork.ConvParticipantRepo.GetAll()
            .Where(cp => cp.ConversationId == conversationId)
            .Select(cp => cp.UserProfileId)
            .ToListAsync();

        // Send to all participants (including sender, for multi-device sync)
        foreach (var participantId in participants)
        {
            await Clients.Group(participantId).SendAsync("ReceiveMessage", messagePayload);
        }
    }

    /// <summary>
    /// Called when user is typing in a conversation.
    /// Notifies the other participant(s).
    /// </summary>
    public async Task SendTypingIndicator(string conversationId)
    {
        var senderProfileId = await GetProfileId();
        if (senderProfileId == null) return;

        var participants = await _unitOfWork.ConvParticipantRepo.GetAll()
            .Where(cp => cp.ConversationId == conversationId && cp.UserProfileId != senderProfileId)
            .Select(cp => cp.UserProfileId)
            .ToListAsync();

        foreach (var participantId in participants)
        {
            await Clients.Group(participantId).SendAsync("UserTyping", conversationId, senderProfileId);
        }
    }

    /// <summary>
    /// Called when user reads messages in a conversation.
    /// </summary>
    public async Task MarkConversationRead(string conversationId)
    {
        var profileId = await GetProfileId();
        if (profileId == null) return;

        await _messageService.MarkAsRead(conversationId, profileId);

        // Notify the sender that their messages were read
        var participants = await _unitOfWork.ConvParticipantRepo.GetAll()
            .Where(cp => cp.ConversationId == conversationId && cp.UserProfileId != profileId)
            .Select(cp => cp.UserProfileId)
            .ToListAsync();

        foreach (var participantId in participants)
        {
            await Clients.Group(participantId).SendAsync("MessagesRead", conversationId, profileId);
        }
    }

    /// <summary>
    /// Returns which of the given profileIds are currently online.
    /// Called by client on page load.
    /// </summary>
    public Task<List<string>> GetOnlineUsers(List<string> profileIds)
    {
        var onlineUsers = new List<string>();
        lock (_lock)
        {
            foreach (var id in profileIds)
            {
                if (_userConnections.ContainsKey(id))
                    onlineUsers.Add(id);
            }
        }
        return Task.FromResult(onlineUsers);
    }

    // ─── Helper ───

    private async Task<string?> GetProfileId()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var userProfile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userProfile?.Id;
    }
}
