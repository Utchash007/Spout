using Twit.UnitOfWork;

namespace Twit.Services
{
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LikePost(string postId)
        {
            var post = await _unitOfWork.PostRepo.Get(postId);
            post.LikesCount++;
            await _unitOfWork.PostRepo.Update(post);
        }

        public async Task LikeComment(string commentId)
        {
            var comment = await _unitOfWork.CommentRepo.Get(commentId);
            comment.LikesCount++;
            await _unitOfWork.CommentRepo.Update(comment);
        }
    }
}
