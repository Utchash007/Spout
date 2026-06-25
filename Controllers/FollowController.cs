using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

public class FollowController : Controller
{
    private readonly IFollowService _followService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileCacheService _userProfileCache;

    public FollowController(IFollowService followService, IUnitOfWork unitOfWork, IUserProfileCacheService userProfileCache)
    {
        _followService = followService;
        _unitOfWork = unitOfWork;
        _userProfileCache = userProfileCache;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _userProfileCache.GetProfileId(userId);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Follow(string profileId)
    {
        var currentId = await GetCurrentUserProfileId();
        if (currentId == null)
            return Unauthorized(new { success = false, message = "Not logged in" });

        await _followService.Follow(currentId, profileId);

        var followersCount = await _followService.GetFollowersCount(profileId);
        var followingCount = await _followService.GetFollowingCount(currentId);

        return Json(new { success = true, isFollowing = true, followersCount = followersCount, followingCount = followingCount });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Unfollow(string profileId)
    {
        var currentId = await GetCurrentUserProfileId();
        if (currentId == null)
            return Unauthorized(new { success = false, message = "Not logged in" });

        await _followService.Unfollow(currentId, profileId);

        var followersCount = await _followService.GetFollowersCount(profileId);
        var followingCount = await _followService.GetFollowingCount(currentId);

        return Json(new { success = true, isFollowing = false, followersCount = followersCount, followingCount = followingCount });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFollow(string profileId)
    {
        var currentId = await GetCurrentUserProfileId();
        if (currentId == null)
            return Unauthorized(new { success = false, message = "Not logged in" });

        var isFollowing = await _followService.IsFollowing(currentId, profileId);
        if (isFollowing)
            await _followService.Unfollow(currentId, profileId);
        else
            await _followService.Follow(currentId, profileId);

        var followersCount = await _followService.GetFollowersCount(profileId);
        var followingCount = await _followService.GetFollowingCount(currentId);

        return Json(new { success = true, isFollowing = !isFollowing, followersCount = followersCount, followingCount = followingCount });
    }
}
