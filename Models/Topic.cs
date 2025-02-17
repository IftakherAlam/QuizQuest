using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace QuizFormsApp.Models
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Navigation property for related templates
        public ICollection<Template> Templates { get; set; } = new List<Template>();
    }
}
