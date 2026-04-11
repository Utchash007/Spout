using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Repository.DBContext;
namespace Twit.Repository.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Like: unique constraint on (UserProfileId, PostId)
        builder.Entity<Like>()
            .HasIndex(l => new { l.UserProfileId, l.PostId })
            .IsUnique()
            .HasFilter("\"PostId\" IS NOT NULL");

        builder.Entity<Like>()
            .HasIndex(l => new { l.UserProfileId, l.CommentId })
            .IsUnique()
            .HasFilter("\"CommentId\" IS NOT NULL");

        // Follow: unique constraint on (FollowerId, FollowingId)
        builder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId })
            .IsUnique();

        // Follow: FK relationships
        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany()
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // ConversationParticipant: composite PK
        builder.Entity<ConversationParticipant>()
            .HasKey(cp => new { cp.ConversationId, cp.UserProfileId });

        // ConversationParticipant: FK relationships
        builder.Entity<ConversationParticipant>()
            .HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ConversationParticipant>()
            .HasOne(cp => cp.UserProfile)
            .WithMany()
            .HasForeignKey(cp => cp.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bookmark: unique constraint on (UserProfileId, PostId)
        builder.Entity<Bookmark>()
            .HasIndex(b => new { b.UserProfileId, b.PostId })
            .IsUnique();

        // Notification: FK relationships
        builder.Entity<Notification>()
            .HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(n => n.Actor)
            .WithMany()
            .HasForeignKey(n => n.ActorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
}
