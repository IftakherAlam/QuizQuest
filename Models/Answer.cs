using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FormId { get; set; }

        [ForeignKey("FormId")]
        public Form Form { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        public string? TextValue { get; set; }  // For SingleLineText & MultiLineText
        public int? IntegerValue { get; set; }  // For Integer Questions
        public bool? BooleanValue { get; set; } // For Checkbox Questions
    }
}
