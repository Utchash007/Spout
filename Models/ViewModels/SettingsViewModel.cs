using System.ComponentModel.DataAnnotations;

namespace Twit.Models.ViewModels
{
    public class SettingsViewModel
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? Website { get; set; }

        public DateTime? DOB { get; set; }

        public string? ProfileImage { get; set; }
        public string? CoverImage { get; set; }

        // Password change fields
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}