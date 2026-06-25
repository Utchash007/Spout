using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMemoryCache _cache;

        public PostService(IUnitOfWork unitOfWork, INotificationService notificationService, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cache = cache;
        }

        private void InvalidateTrendingTagsCache()
        {
            _cache.Remove("trending_tags_5");
            _cache.Remove("trending_tags_10");
        }

        public async Task<Post> CreatePost(string userProfileId, string content)
        {
            var post = new Post
            {
                UserProfileId = userProfileId,
                Content = content
            };

            await _unitOfWork.PostRepo.Add(post);
            await _unitOfWork.SaveChangesAsync();
            InvalidateTrendingTagsCache();
            return post;
        }

        public async Task<Post> EditPost(string postId, string content)
        {
            var post = await _unitOfWork.PostRepo.Get(postId);
            if (post == null) return null;

            post.Content = content;
            await _unitOfWork.PostRepo.Update(post);
            await _unitOfWork.SaveChangesAsync();
            InvalidateTrendingTagsCache();
            return post;
        }

        public async Task<Post> RePost(string userProfileId, string parentPostId)
        {
            var originalPost = await _unitOfWork.PostRepo.Get(parentPostId);
            if (originalPost == null) return null;

            var repost = new Post
            {
                UserProfileId = userProfileId,
                IsRepost = true,
                ParentPostId = parentPostId,
                Content = originalPost.Content
            };

            await _unitOfWork.PostRepo.Add(repost);

            originalPost.RepostCount++;
            await _unitOfWork.PostRepo.Update(originalPost);

            // Trigger notification
            if (originalPost.UserProfileId != userProfileId)
            {
                await _notificationService.CreateNotification(
                    NotificationType.Repost,
                    recipientProfileId: originalPost.UserProfileId,
                    actorProfileId: userProfileId,
                    postId: parentPostId
                );
            }
            
            await _unitOfWork.SaveChangesAsync();
            return repost;
        }

        public async Task<IEnumerable<PostViewModel>> FetchPosts(string? userProfileId = null)
        {
            var rawData = await _unitOfWork.PostRepo.GetAll().AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    Post = p,
                    FirstName = p.UserProfile != null ? p.UserProfile.FirstName : "",
                    LastName = p.UserProfile != null ? p.UserProfile.LastName : "",
                    UserName = (p.UserProfile != null && p.UserProfile.User != null) ? p.UserProfile.User.UserName : "",
                    CommentCount = _unitOfWork.CommentRepo.GetAll().AsNoTracking().Count(c => c.PostId == p.Id),
                    IsLiked = userProfileId != null && 
                        _unitOfWork.LikeRepo.GetAll().AsNoTracking().Any(l => l.UserProfileId == userProfileId && l.PostId == p.Id),
                    IsBookmarked = userProfileId != null && 
                        _unitOfWork.BookmarkRepo.GetAll().AsNoTracking().Any(b => b.UserProfileId == userProfileId && b.PostId == p.Id)
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
                    IsOwnedByCurrentUser = userProfileId != null && d.Post.UserProfileId == userProfileId,
                    IsBookmarkedByCurrentUser = d.IsBookmarked
                };
            }).ToList();
        }

        public async Task DeletePost(string postId)
        {
            await _unitOfWork.PostRepo.Delete(postId);
            await _unitOfWork.SaveChangesAsync();
            InvalidateTrendingTagsCache();
        }
    }
}
