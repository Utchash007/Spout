using Twit.Models;

namespace Twit.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<Conversation>> GetConversations(string profileId);
        Task<IEnumerable<Message>> GetMessages(string conversationId);
        Task<Message> SendMessage(string conversationId, string senderProfileId, string content);
        Task MarkAsRead(string conversationId, string profileId);
        Task<int> GetUnreadCount(string profileId);
        Task<Conversation> GetOrCreateConversation(string profileId1, string profileId2);
    }
}
