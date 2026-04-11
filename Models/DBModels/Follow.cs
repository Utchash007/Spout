using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class Follow
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string FollowerId { get; set; } = string.Empty;

        [Required]
        public string FollowingId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(FollowerId))]
        public UserProfile Follower { get; set; } = null!;

        [ForeignKey(nameof(FollowingId))]
        public UserProfile Following { get; set; } = null!;
    }
}