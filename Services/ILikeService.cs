namespace Twit.Services
{
    public interface ILikeService
    {
        Task LikePost(string postId);
        Task LikeComment(string commentId);
    }
}
