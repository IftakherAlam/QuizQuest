using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizFormsApp.ViewModels
{
    public class FormViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateTitle { get; set; }
        public List<AnswerViewModel> Questions { get; set; } = new List<AnswerViewModel>();
    }

}
