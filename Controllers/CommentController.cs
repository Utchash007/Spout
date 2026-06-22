using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers;

public class CommentController : Controller
{
    private readonly ICommentService _commentService;
    private readonly IUnitOfWork _unitOfWork;

    public CommentController(ICommentService commentService, IUnitOfWork unitOfWork)
    {
        _commentService = commentService;
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(string postId, string content, string? parentCommentId = null)
    {
        var userProfileId = await GetCurrentUserProfileId();

        if (userProfileId == null)
            return RedirectToAction("LoginPage", "Login");

        await _commentService.CreateComment(userProfileId, postId, content, parentCommentId);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(string commentId, string content)
    {
        await _commentService.EditComment(commentId, content);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Delete(string commentId)
    {
        await _commentService.DeleteComment(commentId);
        return RedirectToAction("Index", "Home");
    }
}
