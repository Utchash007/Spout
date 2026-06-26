using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Twit.Models;
using Twit.Services;
using Twit.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Twit.Models.ViewModels;

namespace Twit.Controllers;

public class ProfileController : Controller
{   
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostService _postService;
    private readonly IFollowService _followService;
    private readonly IUserProfileCacheService _userProfileCache;
    private readonly ISidebarService _sidebarService;

    public ProfileController(IUnitOfWork unitOfWork, IPostService postService, IFollowService followService, IUserProfileCacheService userProfileCache, ISidebarService sidebarService)
    {
        _unitOfWork = unitOfWork;
        _postService = postService;
        _followService = followService;
        _userProfileCache = userProfileCache;
        _sidebarService = sidebarService;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _userProfileCache.GetProfileId(userId);
    }

    public async Task<IActionResult> Index(string? Id, string? handle)
    {   
        var currentProfileId = await GetCurrentUserProfileId();
        UserProfile? profile = null;

        if (!string.IsNullOrEmpty(handle))
        {
            profile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.User.UserName == handle);
        }
        else if (Id != null) 
        {
            profile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.Id == Id);
        }
        else
        {
            if (currentProfileId == null) return RedirectToAction("LoginPage", "Login");
            profile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.Id == currentProfileId);
        }

        if (profile == null) return NotFound();

        var posts = await _postService.FetchPosts(currentProfileId);
        var userPosts = posts.Where(p => p.AuthorHandle == profile.User?.UserName)
                             .OrderByDescending(p => p.CreatedAt)
                             .ToList();

        var initials = (profile.FirstName.Length > 0 ? profile.FirstName[0].ToString() : "")
                     + (profile.LastName.Length > 0 ? profile.LastName[0].ToString() : "");

        var model = new ProfileViewModel
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Handle = profile.User?.UserName ?? "",
            Bio = profile.Bio,
            Location = profile.Location,
            Website = profile.Website,
            ProfileImage = profile.ProfileImage,
            CoverImage = profile.CoverImage,
            DOB = profile.DOB,
            CreatedAt = profile.CreatedAt,
            FollowersCount = profile.FollowersCount,
            FollowingCount = profile.FollowingCount,
            PostCount = userPosts.Count,
            Initials = initials.ToUpper(),
            Posts = userPosts,
            IsOwnProfile = profile.Id == currentProfileId
        };

        if (currentProfileId != null && profile.Id != currentProfileId)
        {
            ViewBag.IsFollowing = await _followService.IsFollowing(currentProfileId, profile.Id);
        }
        else
        {
            ViewBag.IsFollowing = false;
        }

        var suggestedUsers = await _sidebarService.GetSuggestedUsers(currentProfileId ?? "", 3);
        ViewBag.SuggestedUsers = suggestedUsers;

        return View(model);
    }
}
