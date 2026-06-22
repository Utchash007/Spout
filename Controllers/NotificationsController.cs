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

    public NotificationsController(INotificationService notificationService, IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var userProfile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userProfile?.Id;
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
