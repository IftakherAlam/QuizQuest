using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuizFormsApp.Models
{
    [PrimaryKey(nameof(TemplateId), nameof(TagId))] // ✅ Composite Primary Key
    public class TemplateTag
    {
        public int TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))] // ✅ Explicit Foreign Key
        public Template Template { get; set; }

        public int TagId { get; set; }

        [ForeignKey(nameof(TagId))] // ✅ Explicit Foreign Key
        public Tag Tag { get; set; }
    }
}
