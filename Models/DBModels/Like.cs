using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class Like
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserProfileId { get; set; } = string.Empty;

        public string? PostId { get; set; }
        public string? CommentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        [ForeignKey(nameof(PostId))]
        public Post? Post { get; set; }

        [ForeignKey(nameof(CommentId))]
        public Comment? Comment { get; set; }
    }
}