namespace Twit.Services
{
    public interface ILikeService
    {
        Task<bool> ToggleLikePost(string userProfileId, string postId);
        Task<bool> ToggleLikeComment(string userProfileId, string commentId);
        Task<bool> HasLikedPost(string userProfileId, string postId);
        Task<bool> HasLikedComment(string userProfileId, string commentId);
    }
}
