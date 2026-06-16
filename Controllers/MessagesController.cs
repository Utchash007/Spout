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
            return RedirectToAction("LoginPage", "Login");

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
            return RedirectToAction("Index");
        }

        if (conversation == null)
            return NotFound();

        var messages = await _messageService.GetMessages(conversationId);
        await _messageService.MarkAsRead(conversationId, profileId);

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
            return RedirectToAction("Index");

        await _messageService.SendMessage(conversationId, profileId, content);
        return RedirectToAction("Chat", new { conversationId });
    }
}
