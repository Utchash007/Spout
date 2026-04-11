using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class Message
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        public string SenderProfileId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ConversationId))]
        public Conversation Conversation { get; set; } = null!;

        [ForeignKey(nameof(SenderProfileId))]
        public UserProfile Sender { get; set; } = null!;
    }
}