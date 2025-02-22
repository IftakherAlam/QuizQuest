using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers
{
    [Authorize(Roles = "Admin,Creator")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuestionController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
[HttpPost]
public async Task<IActionResult> EditQuestion(int questionId, int templateId, string text, string description, QuestionType type, bool isInTable)
{
    var question = await _context.Questions.FindAsync(questionId);
    if (question == null)
    {
        TempData["ErrorMessage"] = "❌ Question not found!";
        return RedirectToAction("AddQuestions", new { templateId });
    }

    question.Text = text;
    question.Description = description;
    question.Type = type;
    question.IsInTable = isInTable;

    _context.Questions.Update(question);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "✅ Question updated successfully!";

    // ✅ Stay on the same page
    return RedirectToAction("AddQuestions", "Question", new { templateId });
}




        // ✅ View: Add Questions to a Template
        public async Task<IActionResult> AddQuestions(int templateId)
{
    var template = await _context.Templates
        .Include(t => t.Questions)
        .FirstOrDefaultAsync(t => t.Id == templateId);

    if (template == null)
    {
        TempData["ErrorMessage"] = "❌ Template not found!";
        return RedirectToAction("Index", "Template");
    }

    return View(template);
}


        // ✅ Add a New Question (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddQuestion(int templateId, string text, string description, QuestionType type, bool isInTable)
        {
            var template = await _context.Templates.FindAsync(templateId);
            if (template == null) return NotFound();

            var question = new Question
            {
                TemplateId = templateId,
                Text = text,
                Description = description,
                Type = type,
                IsInTable = isInTable,
                OrderIndex = template.Questions.Count
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("AddQuestions", new { templateId });
        }

        // ✅ Delete a Question
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int questionId, int templateId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null) return NotFound();

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("AddQuestions", new { templateId });
        }

[HttpDelete("Delete/{id}")]
[HttpPost("Delete/{id}")]
public async Task<IActionResult> Delete(int id)
{
    var question = await _context.Questions.FindAsync(id);
    if (question == null)
        return NotFound();

    _context.Questions.Remove(question);
    await _context.SaveChangesAsync();

    return Ok(); // ✅ Response for successful deletion
}

    }
}
