using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twit.Models
{
    public class ConversationParticipant
    {
        [Required]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        public string UserProfileId { get; set; } = string.Empty;

        [ForeignKey(nameof(ConversationId))]
        public Conversation Conversation { get; set; } = null!;

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;
    }
}