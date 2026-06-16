using Twit.Models;

namespace Twit.Models.ViewModels
{
    public class MessagesViewModel
    {
        public IEnumerable<Conversation> Conversations { get; set; } = new List<Conversation>();
        public Conversation? ActiveConversation { get; set; }
        public IEnumerable<Message> Messages { get; set; } = new List<Message>();
        public string CurrentProfileId { get; set; } = string.Empty;
    }
}
