namespace Twit.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public IEnumerable<PostViewModel> Posts { get; set; } = [];
        public IEnumerable<UserResultViewModel> Users { get; set; } = [];
    }

    public class UserResultViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string Initials { get; set; } = "?";
        public string? Bio { get; set; }
    }
}