namespace Twit.Models.ViewModels
{
    public class PostViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int RepostCount { get; set; }
        public bool IsRepost { get; set; }
        public DateTime CreatedAt { get; set; }

        // Author info (from UserProfile)
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorHandle { get; set; } = string.Empty;
        public string AuthorInitials { get; set; } = "?";
        
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsOwnedByCurrentUser { get; set; }
    }
}
