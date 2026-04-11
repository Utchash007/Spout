using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public enum NotificationType
    {
        Like,
        Follow,
        Repost,
        Mention,
        Comment
    }

    public class Notification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public string RecipientProfileId { get; set; } = string.Empty;

        [Required]
        public string ActorProfileId { get; set; } = string.Empty;

        public string? PostId { get; set; }
        public string? CommentId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RecipientProfileId))]
        public UserProfile Recipient { get; set; } = null!;

        [ForeignKey(nameof(ActorProfileId))]
        public UserProfile Actor { get; set; } = null!;

        [ForeignKey(nameof(PostId))]
        public Post? Post { get; set; }

        [ForeignKey(nameof(CommentId))]
        public Comment? Comment { get; set; }
    }
}