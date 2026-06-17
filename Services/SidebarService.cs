using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.UnitOfWork;

namespace Twit.Services;

public class SidebarService : ISidebarService
{
    private readonly IUnitOfWork _unitOfWork;

    public SidebarService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TrendingTag>> GetTrendingTags(int count = 5)
    {
        var posts = await _unitOfWork.PostRepo.GetAll()
            .Select(p => p.Content)
            .ToListAsync();

        return posts
            .SelectMany(content => Regex.Matches(content, @"#(\w+)"))
            .GroupBy(m => m.Groups[1].Value, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => new TrendingTag { Tag = g.Key, Count = g.Count() })
            .ToList();
    }

    public async Task<IEnumerable<UserProfile>> GetSuggestedUsers(string profileId, int count = 3)
    {
        var followingIds = await _unitOfWork.FollowRepo.GetAll()
            .Where(f => f.FollowerId == profileId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var suggested = await _unitOfWork.UserProfileRepo.GetAll()
            .Include(up => up.User)
            .Where(up => up.Id != profileId && !followingIds.Contains(up.Id))
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .ToListAsync();

        return suggested;
    }
}
