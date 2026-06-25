using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Twit.Models;
using Twit.UnitOfWork;

namespace Twit.Services;

public class SidebarService : ISidebarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public SidebarService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IEnumerable<TrendingTag>> GetTrendingTags(int count = 5)
    {
        var cacheKey = $"trending_tags_{count}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<TrendingTag>? cached) && cached != null)
            return cached;

        var posts = await _unitOfWork.PostRepo.GetAll().AsNoTracking()
            .Select(p => p.Content)
            .ToListAsync();

        var result = posts
            .SelectMany(content => Regex.Matches(content, @"#(\w+)"))
            .GroupBy(m => m.Groups[1].Value, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => new TrendingTag { Tag = g.Key, Count = g.Count() })
            .ToList();

        _cache.Set(cacheKey, (IEnumerable<TrendingTag>)result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<IEnumerable<UserProfile>> GetSuggestedUsers(string profileId, int count = 3)
    {
        var cacheKey = $"suggested_users_{profileId}_{count}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<UserProfile>? cached) && cached != null)
            return cached;

        var followingIds = await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
            .Where(f => f.FollowerId == profileId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var suggested = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
            .Include(up => up.User)
            .Where(up => up.Id != profileId && !followingIds.Contains(up.Id))
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .ToListAsync();

        _cache.Set(cacheKey, (IEnumerable<UserProfile>)suggested, TimeSpan.FromMinutes(2));
        return suggested;
    }
}
