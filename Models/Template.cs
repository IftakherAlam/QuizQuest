using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
    public class Template
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public ApplicationUser Author { get; set; }

        public bool IsPublic { get; set; } = true;

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Please select a topic")]
        public int TopicId { get; set; }

        [ForeignKey("TopicId")]
        public Topic Topic { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TemplateUser>? AllowedUsers { get; set; } = new List<TemplateUser>();
 // ✅ Fixed missing AllowedUsers!

        // ✅ Re-add SearchVector
        [Column(TypeName = "tsvector")]
        public string? SearchVector { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
