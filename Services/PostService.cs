using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public PostService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
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
            return post;
        }

        public async Task<Post> EditPost(string postId, string content)
        {
            var post = await _unitOfWork.PostRepo.Get(postId);
            if (post == null) return null;

            post.Content = content;
            await _unitOfWork.PostRepo.Update(post);
            await _unitOfWork.SaveChangesAsync();
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
            var posts = await _unitOfWork.PostRepo.GetAll()
                .Include(p => p.UserProfile)
                    .ThenInclude(up => up.User)
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
                    IsLikedByCurrentUser = userProfileId != null && 
                        _unitOfWork.LikeRepo.GetAll().Any(l => l.UserProfileId == userProfileId && l.PostId == p.Id),
                    IsOwnedByCurrentUser = userProfileId != null && p.UserProfileId == userProfileId,
                    IsBookmarkedByCurrentUser = userProfileId != null && 
                        _unitOfWork.BookmarkRepo.GetAll().Any(b => b.UserProfileId == userProfileId && b.PostId == p.Id)
                };
            });
        }

        public async Task DeletePost(string postId)
        {
            await _unitOfWork.PostRepo.Delete(postId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
