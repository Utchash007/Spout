using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Twit.Models;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class FollowService : IFollowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMemoryCache _cache;

        public FollowService(IUnitOfWork unitOfWork, INotificationService notificationService, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cache = cache;
        }

        private void InvalidateSuggestedUsersCache(string profileId)
        {
            _cache.Remove($"suggested_users_{profileId}_3");
            _cache.Remove($"suggested_users_{profileId}_5");
        }

        public async Task Follow(string followerProfileId, string followingProfileId)
        {
            if (followerProfileId == followingProfileId) return;

            var exists = await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
                .AnyAsync(f => f.FollowerId == followerProfileId && f.FollowingId == followingProfileId);

            if (exists) return;

            var follow = new Follow
            {
                FollowerId = followerProfileId,
                FollowingId = followingProfileId
            };

            await _unitOfWork.FollowRepo.Add(follow);

            var followingProfile = await _unitOfWork.UserProfileRepo.Get(followingProfileId);
            if (followingProfile != null)
            {
                followingProfile.FollowersCount++;
                await _unitOfWork.UserProfileRepo.Update(followingProfile);
            }

            var followerProfile = await _unitOfWork.UserProfileRepo.Get(followerProfileId);
            if (followerProfile != null)
            {
                followerProfile.FollowingCount++;
                await _unitOfWork.UserProfileRepo.Update(followerProfile);
            }

            // Trigger notification
            await _notificationService.CreateNotification(
                NotificationType.Follow,
                recipientProfileId: followingProfileId,
                actorProfileId: followerProfileId
            );

            await _unitOfWork.SaveChangesAsync();
            InvalidateSuggestedUsersCache(followerProfileId);
        }

        public async Task Unfollow(string followerProfileId, string followingProfileId)
        {
            var follow = await _unitOfWork.FollowRepo.GetAll()
                .FirstOrDefaultAsync(f => f.FollowerId == followerProfileId && f.FollowingId == followingProfileId);

            if (follow == null) return;

            await _unitOfWork.FollowRepo.Delete(follow.Id);

            var followingProfile = await _unitOfWork.UserProfileRepo.Get(followingProfileId);
            if (followingProfile != null)
            {
                followingProfile.FollowersCount = Math.Max(0, followingProfile.FollowersCount - 1);
                await _unitOfWork.UserProfileRepo.Update(followingProfile);
            }

            var followerProfile = await _unitOfWork.UserProfileRepo.Get(followerProfileId);
            if (followerProfile != null)
            {
                followerProfile.FollowingCount = Math.Max(0, followerProfile.FollowingCount - 1);
                await _unitOfWork.UserProfileRepo.Update(followerProfile);
            }

            await _unitOfWork.SaveChangesAsync();
            InvalidateSuggestedUsersCache(followerProfileId);
        }

        public async Task<bool> IsFollowing(string followerProfileId, string followingProfileId)
        {
            return await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
                .AnyAsync(f => f.FollowerId == followerProfileId && f.FollowingId == followingProfileId);
        }

        public async Task<IEnumerable<UserProfile>> GetFollowers(string followingProfileId)
        {
            return await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
                .Where(f => f.FollowingId == followingProfileId)
                .Include(f => f.Follower)
                    .ThenInclude(u => u.User)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserProfile>> GetFollowing(string followerProfileId)
        {
            return await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
                .Where(f => f.FollowerId == followerProfileId)
                .Include(f => f.Following)
                    .ThenInclude(u => u.User)
                .Select(f => f.Following)
                .ToListAsync();
        }

        public async Task<int> GetFollowersCount(string followingProfileId)
        {
            return await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
                .CountAsync(f => f.FollowingId == followingProfileId);
        }

        public async Task<int> GetFollowingCount(string followerProfileId)
        {
            return await _unitOfWork.FollowRepo.GetAll().AsNoTracking()
                .CountAsync(f => f.FollowerId == followerProfileId);
        }
    }
}