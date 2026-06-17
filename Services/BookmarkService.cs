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
        var bookmarkedPostIds = await _unitOfWork.BookmarkRepo.GetAll()
            .Where(b => b.UserProfileId == userProfileId)
            .Select(b => b.PostId)
            .ToListAsync();

        var posts = await _unitOfWork.PostRepo.GetAll()
            .Include(p => p.UserProfile)
                .ThenInclude(up => up.User)
            .Where(p => bookmarkedPostIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(p =>
        {
            var firstName = p.UserProfile?.FirstName ?? "";
            var lastName  = p.UserProfile?.LastName  ?? "";
            var initials  = (firstName.Length > 0 ? firstName[0].ToString() : "?")
                          + (lastName.Length  > 0 ? lastName[0].ToString()  : "");

            return new PostViewModel
            {
                Id           = p.Id,
                Content      = p.Content,
                LikesCount   = p.LikesCount,
                RepostCount  = p.RepostCount,
                IsRepost     = p.IsRepost,
                CreatedAt    = p.CreatedAt,
                AuthorName   = $"{firstName} {lastName}".Trim(),
                AuthorHandle = p.UserProfile?.User?.UserName ?? "",
                AuthorInitials = initials,
                CommentCount = _unitOfWork.CommentRepo.GetAll().Count(c => c.PostId == p.Id),
                IsLikedByCurrentUser = _unitOfWork.LikeRepo.GetAll().Any(l => l.UserProfileId == userProfileId && l.PostId == p.Id),
                IsOwnedByCurrentUser = p.UserProfileId == userProfileId,
                IsBookmarkedByCurrentUser = true
            };
        }).ToList();
    }

    public async Task<bool> IsBookmarked(string userProfileId, string postId)
    {
        return await _unitOfWork.BookmarkRepo.GetAll()
            .AnyAsync(b => b.UserProfileId == userProfileId && b.PostId == postId);
    }
}
