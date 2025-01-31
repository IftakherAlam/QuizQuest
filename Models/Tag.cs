using System.ComponentModel.DataAnnotations;

namespace QuizFormsApp.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
