using Twit.Models;

namespace Twit.Services
{
    public interface ICommentService
    {
        Task<Comment> CreateComment(string userProfileId, string postId, string content, string? parentCommentId = null);
        Task<Comment> EditComment(string commentId, string content);
        Task DeleteComment(string commentId);
    }
}
