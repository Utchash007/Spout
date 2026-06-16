using Twit.Models;
namespace Twit.Services;

public interface IFollowService
{
    Task Follow(string followerProfileId, string followingProfileId);
    Task Unfollow(string followerProfileId, string followingProfileId);
    Task<bool> IsFollowing(string followerProfileId, string followingProfileId);
    Task<IEnumerable<UserProfile>> GetFollowers(string followingProfileId);
    Task<IEnumerable<UserProfile>> GetFollowing(string followerProfileId);
    Task<int> GetFollowersCount(string followingProfileId);
    Task<int> GetFollowingCount(string followerProfileId);
}