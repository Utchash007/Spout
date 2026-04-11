using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class UserProfile
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName {get;set;}
        
        [MaxLength(50)]
        [Required]
        public string LastName {get;set;}

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? Website { get; set; }

        public DateTime? DOB { get; set; }

        [MaxLength(500)]
        public string? ProfileImage { get; set; }

        [MaxLength(500)]
        public string? CoverImage { get; set; }

        public int FollowersCount { get; set; } = 0;

        public int FollowingCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

    }
}