namespace Twit.Services
{
    public interface ILikeService
    {
        Task ToggleLikePost(string userProfileId, string postId);
        Task ToggleLikeComment(string userProfileId, string commentId);
        Task<bool> HasLikedPost(string userProfileId, string postId);
        Task<bool> HasLikedComment(string userProfileId, string commentId);
    }
}
