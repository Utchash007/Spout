using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public NotificationService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<IEnumerable<NotificationViewModel>> GetNotifications(string profileId)
        {
            var notifications = await _unitOfWork.NotificationRepo.GetAll().AsNoTracking()
                .Include(n => n.Actor)
                .Include(n => n.Post)
                .Include(n => n.Comment)
                .Where(n => n.RecipientProfileId == profileId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return notifications.Select(n =>
            {
                var initials = "";
                if (!string.IsNullOrEmpty(n.Actor.FirstName)) initials += n.Actor.FirstName[0];
                if (!string.IsNullOrEmpty(n.Actor.LastName)) initials += n.Actor.LastName[0];

                var timeAgo = (DateTime.UtcNow - n.CreatedAt);
                var timeText = timeAgo.TotalMinutes < 60 ? $"{(int)timeAgo.TotalMinutes}m"
                             : timeAgo.TotalHours < 24 ? $"{(int)timeAgo.TotalHours}h"
                             : $"{(int)timeAgo.TotalDays}d";

                return new NotificationViewModel
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    ActorName = n.Actor.FirstName + " " + n.Actor.LastName,
                    ActorInitials = initials.ToUpper(),
                    PostContent = n.Post?.Content,
                    CommentContent = n.Comment?.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    TimeAgo = timeText
                };
            }).ToList();
        }

        public async Task MarkAsRead(string notificationId)
        {
            var notification = await _unitOfWork.NotificationRepo.Get(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _unitOfWork.NotificationRepo.Update(notification);
                await _unitOfWork.SaveChangesAsync();
                _cache.Remove($"notif_unread_{notification.RecipientProfileId}");
            }
        }

        public async Task MarkAllAsRead(string profileId)
        {
            var notifications = await _unitOfWork.NotificationRepo.GetAll()
                .Where(n => n.RecipientProfileId == profileId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
                await _unitOfWork.NotificationRepo.Update(n);
            }

            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"notif_unread_{profileId}");
        }

        public async Task<int> GetUnreadCount(string profileId)
        {
            var cacheKey = $"notif_unread_{profileId}";
            if (_cache.TryGetValue(cacheKey, out int count))
                return count;

            count = await _unitOfWork.NotificationRepo.GetAll().AsNoTracking()
                .CountAsync(n => n.RecipientProfileId == profileId && !n.IsRead);

            _cache.Set(cacheKey, count, TimeSpan.FromSeconds(30));
            return count;
        }

        public async Task CreateNotification(NotificationType type, string recipientProfileId, string actorProfileId, string? postId = null, string? commentId = null)
        {
            if (recipientProfileId == actorProfileId) return;

            var notification = new Notification
            {
                Type = type,
                RecipientProfileId = recipientProfileId,
                ActorProfileId = actorProfileId,
                PostId = postId,
                CommentId = commentId
            };

            await _unitOfWork.NotificationRepo.Add(notification);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"notif_unread_{recipientProfileId}");
        }
    }
}
