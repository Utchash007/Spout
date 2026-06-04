using Twit.Models;
using Twit.Models.ViewModels;

namespace Twit.Services
{
    public interface ICommentService
    {
        Task<Comment> CreateComment(string userProfileId, string postId, string content, string? parentCommentId = null);
        Task<Comment> EditComment(string commentId, string content);
        Task DeleteComment(string commentId);
        Task<IEnumerable<CommentViewModel>> FetchComments(string postId);
    }
}
