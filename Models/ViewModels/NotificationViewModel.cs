namespace Twit.Models.ViewModels
{
    public class NotificationViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ActorName { get; set; } = string.Empty;
        public string ActorInitials { get; set; } = "?";
        public string? PostContent { get; set; }
        public string? CommentContent { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}