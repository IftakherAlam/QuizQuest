using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{
public class JiraSettings
    {
        public string BaseUrl { get; set; }
        public string Email { get; set; }
        public string ApiToken { get; set; }
        public string ProjectKey { get; set; }
    }
}
