using System.ComponentModel.DataAnnotations;

namespace Twit.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(50)]
        public string FirstName {get;set;}= string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(50)]
        public string LastName {get;set;}= string.Empty;

        [Required(ErrorMessage = "UserName is Required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(30, ErrorMessage = "Username cannot exceed 30 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string UserName {get;set;}=string.Empty;

        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [MaxLength(256)]
        public string Email {get;set;} = string.Empty;

        public DateTime? DOB {get;set;}

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
