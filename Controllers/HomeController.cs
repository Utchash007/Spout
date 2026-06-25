using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileCacheService _userProfileCache;

    public HomeController(IPostService postService, ICommentService commentService, IUnitOfWork unitOfWork, IUserProfileCacheService userProfileCache)
    {
        _postService = postService;
        _commentService = commentService;
        _unitOfWork = unitOfWork;
        _userProfileCache = userProfileCache;
    }

    // Bridges ApplicationUser.Id (from cookie claim) → UserProfile.Id (used in business logic)
    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _userProfileCache.GetProfileId(userId);
    }

    private async Task<string> GetCurrentUserInitials()
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null) return "?";

        var profile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(up => up.Id == profileId);

        if (profile == null) return "?";

        var initials = "";
        if (!string.IsNullOrEmpty(profile.FirstName)) initials += profile.FirstName[0];
        if (!string.IsNullOrEmpty(profile.LastName))  initials += profile.LastName[0];
        return initials.ToUpper();
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var profileId = await GetCurrentUserProfileId();
        var posts = await _postService.FetchPosts(profileId);

        var model = new HomeFeedViewModel
        {
            Posts = posts,
            CurrentUserInitials = await GetCurrentUserInitials()
        };
        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost(string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            var profileId = await GetCurrentUserProfileId();
            if (profileId != null)
                await _postService.CreatePost(profileId, content);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RePost(string postId)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId != null)
            await _postService.RePost(profileId, postId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeletePost(string postId)
    {
        await _postService.DeletePost(postId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditPost(string postId, string content)
    {
        var profileId = await GetCurrentUserProfileId();
        var post = await _unitOfWork.PostRepo.Get(postId);

        if (post == null || post.UserProfileId != profileId)
        {
            return Forbid();
        }

        await _postService.EditPost(postId, content);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [ResponseCache(Duration = 15, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetComments(string postId)
    {
        var comments = await _commentService.FetchComments(postId);
        return Json(comments);
    }

    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
