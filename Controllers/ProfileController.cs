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
    public ProfileController(IUnitOfWork unitOfWork, IPostService postService)
    {
        _unitOfWork = unitOfWork;
        _postService = postService;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var userProfile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userProfile?.Id;
    }

    public async Task<IActionResult> Index(string? Id, string? handle)
    {   
        var currentProfileId = await GetCurrentUserProfileId();
        UserProfile? profile = null;

        if (!string.IsNullOrEmpty(handle))
        {
            profile = await _unitOfWork.UserProfileRepo.GetAll()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.User.UserName == handle);
        }
        else if (Id != null) 
        {
            profile = await _unitOfWork.UserProfileRepo.GetAll()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.Id == Id);
        }
        else
        {
            if (currentProfileId == null) return RedirectToAction("LoginPage", "Login");
            profile = await _unitOfWork.UserProfileRepo.GetAll()
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

        return View(model);
    }
}
