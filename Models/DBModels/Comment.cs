using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class Comment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PostId { get; set; } = string.Empty;

        public string? ParentCommentId { get; set; }

        [Required]
        public string UserProfileId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int LikesCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;

        [ForeignKey(nameof(ParentCommentId))]
        public Comment? ParentComment { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;
    }
}
