using Twit.Models;
using Twit.Models.ViewModels;

namespace Twit.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationViewModel>> GetNotifications(string profileId);
        Task MarkAsRead(string notificationId);
        Task MarkAllAsRead(string profileId);
        Task<int> GetUnreadCount(string profileId);
        Task CreateNotification(NotificationType type, string recipientProfileId, string actorProfileId, string? postId = null, string? commentId = null);
    }
}
