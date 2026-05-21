using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Twit.Models;
using Twit.Repository;
namespace Twit.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<UserProfile> UserProfileRepo { get; }
        IRepository<Post> PostRepo { get; }
        IRepository<Comment> CommentRepo { get; }
        IRepository<Like> LikeRepo { get; }
        IRepository<Follow> FollowRepo { get; }
        IRepository<Notification> NotificationRepo { get; }
        IRepository<Conversation> ConversationRepo { get; }
        IRepository<ConversationParticipant> ConvParticipantRepo { get; }
        IRepository<Message> MessageRepo { get; }
        IRepository<Bookmark> BookmarkRepo { get; }
        Task SaveChangesAsync();
    }   

}