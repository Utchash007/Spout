using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

[Authorize]
public class BookmarkController : Controller
{
    private readonly IBookmarkService _bookmarkService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileCacheService _userProfileCache;

    public BookmarkController(IBookmarkService bookmarkService, IUnitOfWork unitOfWork, IUserProfileCacheService userProfileCache)
    {
        _bookmarkService = bookmarkService;
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

        var bookmarks = await _bookmarkService.GetBookmarks(profileId);
        ViewData["ActiveNav"] = "bookmarks";
        return View(bookmarks);
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(string postId)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null)
            return RedirectToAction("LoginPage", "Login");

        await _bookmarkService.Toggle(profileId, postId);

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }
        return RedirectToAction("Index", "Home");
    }
}
