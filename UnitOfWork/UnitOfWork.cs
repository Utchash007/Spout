using Twit.Models;
using Twit.Repository;
using Twit.Repository.DBContext;
namespace Twit.UnitOfWork
{
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<UserProfile> _profile;
    private IRepository<Post> _post;
    private IRepository<Comment> _comment;
    private IRepository<Like> _like;
    private IRepository<Follow> _follow;
    private IRepository<Notification> _notification;
    private IRepository<Conversation> _conversation;
    private IRepository<ConversationParticipant> _convParticipant;
    private IRepository<Message> _message;
    private IRepository<Bookmark> _bookmark;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<UserProfile> UserProfileRepo
    {
        get
        {
            if (_profile == null)
            {
                _profile = new EFRepository<UserProfile>(_context);
            }
            return _profile;
        }
    }

    public IRepository<Post> PostRepo
    {
        get
        {
            if (_post == null)
            {
                _post = new EFRepository<Post>(_context);
            }
            return _post;
        }
    }

    public IRepository<Comment> CommentRepo
    {
        get
        {
            if (_comment == null)
            {
                _comment = new EFRepository<Comment>(_context);
            }
            return _comment;
        }
    }

    public IRepository<Like> LikeRepo
    {
        get
        {
            if (_like == null)
            {
                _like = new EFRepository<Like>(_context);
            }
            return _like;
        }
    }

    public IRepository<Follow> FollowRepo
    {
        get
        {
            if (_follow == null)
            {
                _follow = new EFRepository<Follow>(_context);
            }
            return _follow;
        }
    }

    public IRepository<Notification> NotificationRepo
    {
        get
        {
            if (_notification == null)
            {
                _notification = new EFRepository<Notification>(_context);
            }
            return _notification;
        }
    }

    public IRepository<Conversation> ConversationRepo
    {
        get
        {
            if (_conversation == null)
            {
                _conversation = new EFRepository<Conversation>(_context);
            }
            return _conversation;
        }
    }

    public IRepository<ConversationParticipant> ConvParticipantRepo
    {
        get
        {
            if (_convParticipant == null)
            {
                _convParticipant = new EFRepository<ConversationParticipant>(_context);
            }
            return _convParticipant;
        }
    }

    public IRepository<Message> MessageRepo
    {
        get
        {
            if (_message == null)
            {
                _message = new EFRepository<Message>(_context);
            }
            return _message;
        }
    }

    public IRepository<Bookmark> BookmarkRepo
    {
        get
        {
            if (_bookmark == null)
            {
                _bookmark = new EFRepository<Bookmark>(_context);
            }
            return _bookmark;
        }
    }
}
}
