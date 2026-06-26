using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookmarkService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Toggle(string userProfileId, string postId)
    {
        var existing = await _unitOfWork.BookmarkRepo.GetAll()
            .FirstOrDefaultAsync(b => b.UserProfileId == userProfileId && b.PostId == postId);

        if (existing != null)
        {
            await _unitOfWork.BookmarkRepo.Delete(existing.Id);
            await _unitOfWork.SaveChangesAsync();
            return false;
        }

        var bookmark = new Bookmark
        {
            UserProfileId = userProfileId,
            PostId = postId
        };

        await _unitOfWork.BookmarkRepo.Add(bookmark);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PostViewModel>> GetBookmarks(string userProfileId)
    {
        var bookmarkedPostIds = await _unitOfWork.BookmarkRepo.GetAll().AsNoTracking()
            .Where(b => b.UserProfileId == userProfileId)
            .Select(b => b.PostId)
            .ToListAsync();

        var rawData = await _unitOfWork.PostRepo.GetAll().AsNoTracking()
            .Where(p => bookmarkedPostIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                Post = p,
                FirstName = p.UserProfile != null ? p.UserProfile.FirstName : "",
                LastName = p.UserProfile != null ? p.UserProfile.LastName : "",
                UserName = (p.UserProfile != null && p.UserProfile.User != null) ? p.UserProfile.User.UserName : "",
                CommentCount = _unitOfWork.CommentRepo.GetAll().AsNoTracking().Count(c => c.PostId == p.Id),
                IsLiked = _unitOfWork.LikeRepo.GetAll().AsNoTracking().Any(l => l.UserProfileId == userProfileId && l.PostId == p.Id)
            })
            .ToListAsync();

        return rawData.Select(d =>
        {
            var firstName = d.FirstName ?? "";
            var lastName  = d.LastName ?? "";
            var initials  = (firstName.Length > 0 ? firstName[0].ToString() : "?")
                          + (lastName.Length  > 0 ? lastName[0].ToString()  : "");

            return new PostViewModel
            {
                Id           = d.Post.Id,
                Content      = d.Post.Content,
                LikesCount   = d.Post.LikesCount,
                RepostCount  = d.Post.RepostCount,
                IsRepost     = d.Post.IsRepost,
                CreatedAt    = d.Post.CreatedAt,
                AuthorName   = $"{firstName} {lastName}".Trim(),
                AuthorHandle = d.UserName,
                AuthorInitials = initials,
                CommentCount = d.CommentCount,
                IsLikedByCurrentUser = d.IsLiked,
                IsOwnedByCurrentUser = d.Post.UserProfileId == userProfileId,
                IsBookmarkedByCurrentUser = true
            };
        }).ToList();
    }

    public async Task<bool> IsBookmarked(string userProfileId, string postId)
    {
        return await _unitOfWork.BookmarkRepo.GetAll().AsNoTracking()
            .AnyAsync(b => b.UserProfileId == userProfileId && b.PostId == postId);
    }
}
