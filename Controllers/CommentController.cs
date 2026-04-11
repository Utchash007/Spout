using Microsoft.AspNetCore.Mvc;
using Twit.Services;

namespace Twit.Controllers;

public class CommentController : Controller
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string postId, string content, string? parentCommentId = null)
    {
        // TODO: Replace with actual logged-in user's profile ID once auth is wired
        var userProfileId = "temp-user";
        await _commentService.CreateComment(userProfileId, postId, content, parentCommentId);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string commentId, string content)
    {
        await _commentService.EditComment(commentId, content);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string commentId)
    {
        await _commentService.DeleteComment(commentId);
        return RedirectToAction("Index", "Home");
    }
}
