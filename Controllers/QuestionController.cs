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

        // ✅ View: Add Questions to a Template
        public async Task<IActionResult> AddQuestions(int templateId)
        {
            var template = await _context.Templates.Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) return NotFound();

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
    }
}
