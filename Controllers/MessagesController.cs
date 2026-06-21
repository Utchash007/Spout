using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IUnitOfWork _unitOfWork;

    public MessagesController(IMessageService messageService, IUnitOfWork unitOfWork)
    {
        _messageService = messageService;
        _unitOfWork = unitOfWork;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var userProfile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userProfile?.Id;
    }

    public async Task<IActionResult> Index()
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null)
            return RedirectToAction("LoginPage", "Login");

        var conversations = await _messageService.GetConversations(profileId);
        
        var viewModel = new MessagesViewModel
        {
            Conversations = conversations,
            CurrentProfileId = profileId,
            ActiveConversation = null,
            Messages = new List<Message>()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Chat(string? conversationId, string? withProfileId)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Unauthorized();
            return RedirectToAction("LoginPage", "Login");
        }

        Conversation? conversation = null;

        if (!string.IsNullOrEmpty(conversationId))
        {
            conversation = await _unitOfWork.ConversationRepo.GetAll()
                .Include(c => c.Participants)
                .ThenInclude(p => p.UserProfile)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }
        else if (!string.IsNullOrEmpty(withProfileId))
        {
            conversation = await _messageService.GetOrCreateConversation(profileId, withProfileId);
            conversation = await _unitOfWork.ConversationRepo.GetAll()
                .Include(c => c.Participants)
                .ThenInclude(p => p.UserProfile)
                .FirstOrDefaultAsync(c => c.Id == conversation.Id);
            conversationId = conversation?.Id;
        }
        else
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return BadRequest("Conversation ID or Profile ID is required.");
            return RedirectToAction("Index");
        }

        if (conversation == null)
            return NotFound();

        var messages = await _messageService.GetMessages(conversationId);
        await _messageService.MarkAsRead(conversationId, profileId);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var otherUser = conversation.Participants.FirstOrDefault(p => p.UserProfileId != profileId)?.UserProfile;
            var partnerInitials = "";
            if (otherUser != null)
            {
                if (!string.IsNullOrEmpty(otherUser.FirstName)) partnerInitials += otherUser.FirstName[0];
                if (!string.IsNullOrEmpty(otherUser.LastName)) partnerInitials += otherUser.LastName[0];
                partnerInitials = partnerInitials.ToUpper();
            }

            var jsonMessages = new List<object>();
            DateTime? lastDate = null;
            foreach (var msg in messages)
            {
                var isSent = msg.SenderProfileId == profileId;
                var senderInitials = "";
                if (!isSent && msg.Sender != null)
                {
                    if (!string.IsNullOrEmpty(msg.Sender.FirstName)) senderInitials += msg.Sender.FirstName[0];
                    if (!string.IsNullOrEmpty(msg.Sender.LastName)) senderInitials += msg.Sender.LastName[0];
                    senderInitials = senderInitials.ToUpper();
                }

                string? dateDividerText = null;
                if (lastDate == null || lastDate.Value.Date != msg.CreatedAt.Date)
                {
                    lastDate = msg.CreatedAt;
                    dateDividerText = msg.CreatedAt.ToString("MMMM dd, yyyy");
                }

                jsonMessages.Add(new
                {
                    id = msg.Id,
                    content = msg.Content,
                    createdAtFormatted = msg.CreatedAt.ToString("h:mm tt"),
                    dateDivider = dateDividerText,
                    isSent = isSent,
                    senderInitials = senderInitials
                });
            }

            return Json(new
            {
                conversationId = conversation.Id,
                currentProfileId = profileId,
                partnerName = otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}" : "Unknown User",
                partnerInitials = partnerInitials,
                partnerProfileId = otherUser?.Id ?? "",
                participantIds = conversation.Participants.Select(p => p.UserProfileId).ToList(),
                messages = jsonMessages
            });
        }

        var conversations = await _messageService.GetConversations(profileId);

        var viewModel = new MessagesViewModel
        {
            Conversations = conversations,
            CurrentProfileId = profileId,
            ActiveConversation = conversation,
            Messages = messages
        };

        return View("Index", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Send(string conversationId, string content)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null || string.IsNullOrWhiteSpace(content))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return BadRequest("Invalid message content");
            return RedirectToAction("Index");
        }

        var message = await _messageService.SendMessage(conversationId, profileId, content);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                id = message.Id,
                content = message.Content,
                createdAtFormatted = message.CreatedAt.ToString("h:mm tt"),
                isSent = true
            });
        }

        return RedirectToAction("Chat", new { conversationId });
    }
}
