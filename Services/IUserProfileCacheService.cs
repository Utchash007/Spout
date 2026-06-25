namespace Twit.Services;

public interface IUserProfileCacheService
{
    Task<string?> GetProfileId(string userId);
    void InvalidateProfileId(string userId);
}
