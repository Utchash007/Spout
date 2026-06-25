using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Models.ViewModels;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

public class ExploreController : Controller
{
    private readonly IPostService _postService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISidebarService _sidebarService;
    private readonly IUserProfileCacheService _userProfileCache;

    public ExploreController(IPostService postService, IUnitOfWork unitOfWork, ISidebarService sidebarService, IUserProfileCacheService userProfileCache)
    {
        _postService = postService;
        _unitOfWork = unitOfWork;
        _sidebarService = sidebarService;
        _userProfileCache = userProfileCache;
    }

    public async Task<IActionResult> Index()
    {
        var profileId = await GetCurrentUserProfileId();
        var posts = await _postService.FetchPosts(profileId);
        var trending = posts.OrderByDescending(p => p.LikesCount + p.RepostCount).Take(20).ToList();

        var suggestedUsers = Enumerable.Empty<Twit.Models.UserProfile>();
        if (profileId != null)
        {
            suggestedUsers = await _sidebarService.GetSuggestedUsers(profileId, 5);
        }
        else
        {
            suggestedUsers = await _sidebarService.GetSuggestedUsers("", 5);
        }

        ViewBag.TrendingPosts = trending;
        ViewBag.SuggestedUsers = suggestedUsers;
        
        ViewData["ActiveNav"] = "explore";
        return View();
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _userProfileCache.GetProfileId(userId);
    }
}
