using Microsoft.AspNetCore.Mvc;
using Twit.Services;

namespace Twit.Controllers;

public class LikeController : Controller
{
    private readonly ILikeService _likeService;

    public LikeController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost]
    public async Task<IActionResult> LikePost(string postId)
    {
        await _likeService.LikePost(postId);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> LikeComment(string commentId)
    {
        await _likeService.LikeComment(commentId);
        return RedirectToAction("Index", "Home");
    }
}
