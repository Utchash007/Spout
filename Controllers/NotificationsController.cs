using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Models.ViewModels;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileCacheService _userProfileCache;

    public NotificationsController(INotificationService notificationService, IUnitOfWork unitOfWork, IUserProfileCacheService userProfileCache)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _userProfileCache = userProfileCache;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _userProfileCache.GetProfileId(userId);
    }

    public async Task<IActionResult> Index()
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null)
            return RedirectToAction("LoginPage", "Login");

        var notifications = await _notificationService.GetNotifications(profileId);
        var unreadCount = await _notificationService.GetUnreadCount(profileId);

        ViewBag.UnreadCount = unreadCount;
        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentNotifications()
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null) return Unauthorized();

        var notifications = await _notificationService.GetNotifications(profileId);
        var recent = notifications.Take(5).Select(n => new
        {
            n.Id,
            n.Type,
            n.ActorName,
            n.ActorInitials,
            n.TimeAgo,
            n.IsRead
        });

        return Json(recent);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        await _notificationService.MarkAsRead(notificationId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId != null)
            await _notificationService.MarkAllAsRead(profileId);
        return RedirectToAction("Index");
    }
}
