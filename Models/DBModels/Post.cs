using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class Post
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserProfileId { get; set; } = string.Empty;

        public bool IsRepost { get; set; } = false;

        public string? ParentPostId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int LikesCount { get; set; } = 0;

        public int RepostCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        [ForeignKey(nameof(ParentPostId))]
        public Post? ParentPost { get; set; }
    }
}
