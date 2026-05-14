using Microsoft.AspNetCore.Mvc;
using Twit.Services;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims;
namespace Twit.Controllers;
using Twit.UnitOfWork;
using Microsoft.EntityFrameworkCore;
public class LikeController : Controller
{
    private readonly ILikeService _likeService;
    private readonly IUnitOfWork _unitOfWork;
    public LikeController(IUnitOfWork unitOfWork ,ILikeService likeService)
    {
        _likeService = likeService;
        _unitOfWork = unitOfWork;
    }
    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // ApplicationUser.Id
        if (userId == null) return null;

        var profile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return profile?.Id; // UserProfile.Id
    }
    [HttpPost]
    [Authorize]

    public async Task<IActionResult> LikePost(string postId)
        {
            await _likeService.ToggleLikePost(postId, await GetCurrentUserProfileId());
            return RedirectToAction("Index", "Home");
        }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LikeComment(string commentId)
    {
            await _likeService.ToggleLikeComment(commentId, await GetCurrentUserProfileId());
            return RedirectToAction("Index", "Home");
    }
}

    
