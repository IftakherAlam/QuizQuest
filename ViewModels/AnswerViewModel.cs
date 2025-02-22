using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizFormsApp.ViewModels
{
    public class AnswerViewModel
{
    [Required]
    public int QuestionId { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? Answer { get; set; }
    public bool CheckboxAnswer { get; set; }
}

}
