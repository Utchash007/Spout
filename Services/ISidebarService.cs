using Twit.Models;

namespace Twit.Services;

public interface ISidebarService
{
    Task<IEnumerable<TrendingTag>> GetTrendingTags(int count = 5);
    Task<IEnumerable<UserProfile>> GetSuggestedUsers(string profileId, int count = 3);
}

public class TrendingTag
{
    public string Tag { get; set; } = string.Empty;
    public int Count { get; set; }
}
