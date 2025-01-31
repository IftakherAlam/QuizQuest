using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        // Add Question (GET)
        public IActionResult Create(int templateId)
        {
            var question = new Question { TemplateId = templateId };
            return View(question);
        }

        // Add Question (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Question question)
        {
            if (ModelState.IsValid)
            {
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", "Template", new { id = question.TemplateId });
            }
            return View(question);
        }
    }
}
