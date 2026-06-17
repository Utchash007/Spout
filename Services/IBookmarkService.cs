using Twit.Models.ViewModels;

namespace Twit.Services;

public interface IBookmarkService
{
    Task<bool> Toggle(string userProfileId, string postId);
    Task<IEnumerable<PostViewModel>> GetBookmarks(string userProfileId);
    Task<bool> IsBookmarked(string userProfileId, string postId);
}
