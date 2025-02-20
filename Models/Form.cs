using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
    public class Form
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [ForeignKey("TemplateId")]
        public Template Template { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();

         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
