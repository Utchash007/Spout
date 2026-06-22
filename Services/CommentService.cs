using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public CommentService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Comment> CreateComment(string userProfileId, string postId, string content, string? parentCommentId = null)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid().ToString(),
                UserProfileId = userProfileId,
                PostId = postId,
                Content = content,
                ParentCommentId = parentCommentId,
                LikesCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CommentRepo.Add(comment);

            // Trigger notification
            var post = await _unitOfWork.PostRepo.Get(postId);
            if (post != null && post.UserProfileId != userProfileId)
            {
                await _notificationService.CreateNotification(
                    NotificationType.Comment,
                    recipientProfileId: post.UserProfileId,
                    actorProfileId: userProfileId,
                    postId: postId,
                    commentId: comment.Id
                );
            }

            await _unitOfWork.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> EditComment(string commentId, string content)
        {
            var comment = await _unitOfWork.CommentRepo.Get(commentId);
            if (comment == null) return null;

            comment.Content = content;
            await _unitOfWork.CommentRepo.Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return comment;
        }

        public async Task DeleteComment(string commentId)
        {
            await _unitOfWork.CommentRepo.Delete(commentId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CommentViewModel>> FetchComments(string postId)
        {
            var comments = await _unitOfWork.CommentRepo.GetAll().AsNoTracking()
                .Include(c => c.UserProfile)
                    .ThenInclude(up => up.User)
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c =>
            {
                var initials = "";
                if (!string.IsNullOrEmpty(c.UserProfile?.FirstName)) initials += c.UserProfile.FirstName[0];
                if (!string.IsNullOrEmpty(c.UserProfile?.LastName)) initials += c.UserProfile.LastName[0];

                return new CommentViewModel
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    Content = c.Content,
                    LikesCount = c.LikesCount,
                    CreatedAt = c.CreatedAt,
                    AuthorName = $"{c.UserProfile?.FirstName} {c.UserProfile?.LastName}".Trim(),
                    AuthorInitials = initials.Length > 0 ? initials.ToUpper() : "?"
                };
            }).ToList();
        }
    }
}
