using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuizFormsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string DisplayName { get; set; } = string.Empty;

        public string PreferredLanguage { get; set; } = "en";
        public string Theme { get; set; } = "light";

        public ICollection<Template> Templates { get; set; }
        public ICollection<Form> Forms { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
