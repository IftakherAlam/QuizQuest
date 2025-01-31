using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [ForeignKey("TemplateId")]
        public Template Template { get; set; }

        [Required, MaxLength(255)]
        public string Text { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public QuestionType Type { get; set; }

        public bool IsInTable { get; set; } = false;

        public int OrderIndex { get; set; }
    }

    public enum QuestionType
    {
        SingleLineText,
        MultiLineText,
        Integer,
        Checkbox
    }
}
