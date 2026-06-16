using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<NotificationViewModel>> GetNotifications(string profileId)
        {
            var notifications = await _unitOfWork.NotificationRepo.GetAll()
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
        }

        public async Task<int> GetUnreadCount(string profileId)
        {
            return await _unitOfWork.NotificationRepo.GetAll()
                .CountAsync(n => n.RecipientProfileId == profileId && !n.IsRead);
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
        }
    }
}
