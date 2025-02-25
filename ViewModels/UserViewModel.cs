using System.Collections.Generic;

namespace QuizFormsApp.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsBlocked { get; set; }
    }
}
