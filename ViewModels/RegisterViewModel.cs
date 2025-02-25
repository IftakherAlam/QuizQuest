using System.ComponentModel.DataAnnotations;

namespace QuizFormsApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")] // ✅ Add label for UI
        public string DisplayName { get; set; }  // ✅ Add this line

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
