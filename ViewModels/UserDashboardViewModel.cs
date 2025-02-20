using System.Collections.Generic;

namespace QuizFormsApp.Models
{
    public class UserDashboardViewModel
    {
        public List<Template> LikedTemplates { get; set; } = new List<Template>();
        public List<Form> SubmittedForms { get; set; } = new List<Form>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
