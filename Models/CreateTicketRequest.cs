using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
   public class CreateTicketRequest
    {
        public string Summary { get; set; }
        public string Priority { get; set; }
        public string Template { get; set; }
        public string PageLink { get; set; }
        public string ReporterEmail { get; set; }
    }
}
