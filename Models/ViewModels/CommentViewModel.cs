namespace Twit.Models.ViewModels
{
    public class CommentViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string PostId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Author info (from UserProfile)
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorInitials { get; set; } = "?";
    }
}
