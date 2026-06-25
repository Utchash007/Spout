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
    private readonly IUserProfileCacheService _userProfileCache;

    public LikeController(IUnitOfWork unitOfWork, ILikeService likeService, IUserProfileCacheService userProfileCache)
    {
        _likeService = likeService;
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
    public async Task<IActionResult> LikePost(string postId)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null) return Unauthorized();

        var isLiked = await _likeService.ToggleLikePost(profileId, postId);
        
        // Fetch the updated count
        var post = await _unitOfWork.PostRepo.Get(postId);
        
        return Json(new { 
            success = true, 
            isLiked = isLiked, 
            count = post?.LikesCount ?? 0 
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LikeComment(string commentId)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null) return Unauthorized();

        var isLiked = await _likeService.ToggleLikeComment(profileId, commentId);
        
        // Fetch the updated count
        var comment = await _unitOfWork.CommentRepo.Get(commentId);

        return Json(new { 
            success = true, 
            isLiked = isLiked, 
            count = comment?.LikesCount ?? 0 
        });
    }
}

    
