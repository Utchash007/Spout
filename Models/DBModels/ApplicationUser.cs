using Microsoft.AspNetCore.Identity;

namespace Twit.Models
{
    public class ApplicationUser : IdentityUser
    {
        public UserProfile UserProfile { get; set; } = null!;
    }
}