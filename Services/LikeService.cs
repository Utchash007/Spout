using Twit.UnitOfWork;
using Twit.Models;
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

        public async Task<bool> ToggleLikePost(string userProfileId, string postId)
        {
            var post = await _unitOfWork.PostRepo.Get(postId);
            if (post == null) return false;

            var existingLike = await _unitOfWork.LikeRepo.GetAll()
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserProfileId == userProfileId);
            
            bool isLiked;
            if (existingLike != null)
            {
                await _unitOfWork.LikeRepo.Delete(existingLike.Id);
                post.LikesCount = Math.Max(0, post.LikesCount - 1);
                isLiked = false;
            }
            else
            {
                var like = new Like
                {
                    UserProfileId = userProfileId,
                    PostId = postId
                };
                await _unitOfWork.LikeRepo.Add(like);
                post.LikesCount++;
                isLiked = true;
            }
            
            await _unitOfWork.PostRepo.Update(post);
            await _unitOfWork.SaveChangesAsync(); // Single save for all changes
            return isLiked;
        }

        public async Task<bool> ToggleLikeComment(string userProfileId, string commentId)
        {
            var comment = await _unitOfWork.CommentRepo.Get(commentId);
            if (comment == null) return false;

            var existingLike = await _unitOfWork.LikeRepo.GetAll()
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserProfileId == userProfileId);
            
            bool isLiked;
            if (existingLike != null)
            {
                await _unitOfWork.LikeRepo.Delete(existingLike.Id);
                comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
                isLiked = false;
            }
            else
            {
                var like = new Like
                {
                    UserProfileId = userProfileId,
                    CommentId = commentId
                };
                await _unitOfWork.LikeRepo.Add(like);
                comment.LikesCount++;
                isLiked = true;
            }
            
            await _unitOfWork.CommentRepo.Update(comment);
            await _unitOfWork.SaveChangesAsync(); // Single save for all changes
            return isLiked;
        }

        public async Task<bool> HasLikedPost(string userProfileId, string postId)
        {
            return await _unitOfWork.LikeRepo.GetAll()
                .AnyAsync(p => p.PostId == postId && p.UserProfileId == userProfileId);
        }

        public async Task<bool> HasLikedComment(string userProfileId, string commentId)
        {
            return await _unitOfWork.LikeRepo.GetAll()
                .AnyAsync(c => c.CommentId == commentId && c.UserProfileId == userProfileId);
        }
    }
}
