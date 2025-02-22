using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizFormsApp.ViewModels
{
   public class FormViewModel
{
    [Required]
    public int TemplateId { get; set; }

    [Required]
    public string TemplateTitle { get; set; } = string.Empty;

    [Required]
    public List<AnswerViewModel> Questions { get; set; } = new List<AnswerViewModel>();
}


}
