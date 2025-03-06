using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
   public class JiraTicket
    {
        public string Key { get; set; }
        public string Summary { get; set; }
        public string Status { get; set; }
        public string Link { get; set; }
    }
}
