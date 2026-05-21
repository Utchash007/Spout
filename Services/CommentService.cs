using Twit.Models;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
