namespace QuizFormsApp.ViewModels
{
    public class AnswerViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Description { get; set; } = "";
        public string Type { get; set; }

        // ✅ Ensure Answer is assigned a default value
        public string? Answer { get; set; } = null;
        public bool CheckboxAnswer { get; set; } = false; // ✅ NEW: For Checkbox
    }
}
