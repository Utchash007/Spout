using Twit.UnitOfWork;
using Microsoft.EntityFrameworkCore;
namespace Twit.Services
{
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ToggleLikePost(string postId, string userId)
        {
            var post = await _unitOfWork.PostRepo.Get(postId);
            var like = await _unitOfWork.LikeRepo.GetAll().FirstOrDefaultAsync(l => l.Id == postId && l.UserProfileId == userId);
            if (like != null)
            {
                await _unitOfWork.LikeRepo.Delete(like.Id);
                post.LikesCount= Math.Max(0, post.LikesCount - 1);
                await _unitOfWork.PostRepo.Update(post);
                return;
            }
            post.LikesCount++;
            await _unitOfWork.PostRepo.Update(post);
        }

        public async Task ToggleLikeComment(string commentId, string userId)
        {
            var comment = await _unitOfWork.CommentRepo.Get(commentId);
            var like = await _unitOfWork.LikeRepo.GetAll().FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserProfileId == userId);
            if (like != null)
            {
                await _unitOfWork.LikeRepo.Delete(like.Id);
                comment.LikesCount= Math.Max(0, comment.LikesCount - 1);
                await _unitOfWork.CommentRepo.Update(comment);
                return;
            }
            comment.LikesCount++;
            await _unitOfWork.CommentRepo.Update(comment);
        }

        public async Task<bool> HasLikedPost(string postId, string userId)
        {
            bool isLiked = _unitOfWork.LikeRepo.GetAll().Where(p => p.Id == postId && p.UserProfileId == userId).Any();
            return isLiked;
        }

        public async Task<bool> HasLikedComment(string commentId, string userId)
        {
            bool isLiked = _unitOfWork.LikeRepo.GetAll().Where(c => c.CommentId==commentId && c.UserProfileId==userId).Any();
            return isLiked;
        }
    }
}
