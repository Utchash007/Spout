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
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IPostService postService, IUnitOfWork unitOfWork)
    {
        _postService = postService;
        _unitOfWork = unitOfWork;
    }

    // Bridges ApplicationUser.Id (from cookie claim) → UserProfile.Id (used in business logic)
    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // ApplicationUser.Id
        if (userId == null) return null;

        var profile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return profile?.Id; // UserProfile.Id
    }

    private async Task<string> GetCurrentUserInitials()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return "?";

        var profile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (profile == null) return "?";

        var initials = "";
        if (!string.IsNullOrEmpty(profile.FirstName)) initials += profile.FirstName[0];
        if (!string.IsNullOrEmpty(profile.LastName))  initials += profile.LastName[0];
        return initials.ToUpper();
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _postService.FetchPosts();
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
