using Twit.Models;
using Twit.Models.ViewModels;

namespace Twit.Services
{
    public interface IPostService
    {
        Task<Post> CreatePost(string userProfileId, string content);
        Task<Post> EditPost(string postId, string content);
        Task<Post> RePost(string userProfileId, string parentPostId);
        Task<IEnumerable<PostViewModel>> FetchPosts();
        Task DeletePost(string postId);
    }
}
