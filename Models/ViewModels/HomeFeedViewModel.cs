namespace Twit.Models.ViewModels
{
    public class HomeFeedViewModel
    {
        public IEnumerable<PostViewModel> Posts { get; set; } = [];
        public string CurrentUserInitials { get; set; } = "?";
    }
}
