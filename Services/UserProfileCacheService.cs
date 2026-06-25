using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Twit.UnitOfWork;

namespace Twit.Services;

public class UserProfileCacheService : IUserProfileCacheService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public UserProfileCacheService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<string?> GetProfileId(string userId)
    {
        var cacheKey = $"user_profile_id_{userId}";

        if (_cache.TryGetValue(cacheKey, out string? profileId))
            return profileId;

        var profile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        profileId = profile?.Id;
        _cache.Set(cacheKey, profileId, TimeSpan.FromMinutes(2));
        return profileId;
    }

    public void InvalidateProfileId(string userId)
    {
        _cache.Remove($"user_profile_id_{userId}");
    }
}
