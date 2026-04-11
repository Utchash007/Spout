namespace Twit.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? ProfileImage { get; set; }
        public string? CoverImage { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostCount { get; set; }
        public string Initials { get; set; } = "?";
        public IEnumerable<PostViewModel> Posts { get; set; } = [];
        public bool IsOwnProfile { get; set; }
    }
}