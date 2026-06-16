using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Conversation>> GetConversations(string profileId)
        {
            var conversations = await _unitOfWork.ConversationRepo.GetAll()
                .Include(c => c.Participants)
                .ThenInclude(p => p.UserProfile)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserProfileId == profileId))
                .OrderByDescending(c => c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault())
                .ToListAsync();

            return conversations;
        }

        public async Task<IEnumerable<Message>> GetMessages(string conversationId)
        {
            return await _unitOfWork.MessageRepo.GetAll()
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message> SendMessage(string conversationId, string senderProfileId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderProfileId = senderProfileId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MessageRepo.Add(message);
            await _unitOfWork.SaveChangesAsync();
            return message;
        }

        public async Task MarkAsRead(string conversationId, string profileId)
        {
            await _unitOfWork.MessageRepo.GetAll()
                .Where(m => m.ConversationId == conversationId && m.SenderProfileId != profileId && !m.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsRead, true));
        }

        public async Task<int> GetUnreadCount(string profileId)
        {
            return await _unitOfWork.MessageRepo.GetAll()
                .CountAsync(m => m.SenderProfileId != profileId && !m.IsRead
                    && _unitOfWork.ConversationRepo.GetAll()
                        .Any(c => c.Id == m.ConversationId && c.Participants.Any(p => p.UserProfileId == profileId)));
        }

        public async Task<Conversation> GetOrCreateConversation(string profileId1, string profileId2)
        {
            var existing = await _unitOfWork.ConversationRepo.GetAll()
                .Include(c => c.Participants)
                .Where(c => c.Participants.Count(p => p.UserProfileId == profileId1 || p.UserProfileId == profileId2) == 2)
                .FirstOrDefaultAsync();

            if (existing != null) return existing;

            var conversation = new Conversation
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            
            conversation.Participants.Add(new ConversationParticipant { UserProfileId = profileId1 });
            conversation.Participants.Add(new ConversationParticipant { UserProfileId = profileId2 });

            await _unitOfWork.ConversationRepo.Add(conversation);
            await _unitOfWork.SaveChangesAsync();

            return conversation;
        }
    }
}
