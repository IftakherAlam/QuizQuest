using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using QuizFormsApp.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    public class FormController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FormController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Show Form for Submission (Merged Logic)
        public async Task<IActionResult> Fill(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.AllowedUsers) // ✅ Load allowed users for access control
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            // ✅ Check if user has access
            var user = await _userManager.GetUserAsync(User);
            if (!template.IsPublic && !template.AllowedUsers.Any(u => u.UserId == user.Id))
                return Forbid();

            var viewModel = new FormViewModel
            {
                TemplateId = template.Id,
                TemplateTitle = template.Title,
                Questions = template.Questions.Select(q => new AnswerViewModel
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    Type = q.Type.ToString()
                }).ToList()
            };

            return View(viewModel);
        }

        // ✅ Submit Form
        [HttpPost]
        public async Task<IActionResult> Submit(FormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Fill", model);

            var user = await _userManager.GetUserAsync(User);
            var form = new Form
            {
                TemplateId = model.TemplateId,
                UserId = user.Id,
                SubmissionDate = DateTime.UtcNow
            };

            _context.Forms.Add(form);
            await _context.SaveChangesAsync();

            var answers = model.Questions.Select(q => new Answer
            {
                FormId = form.Id,
                QuestionId = q.QuestionId,
                Value = q.Answer
            }).ToList();

            _context.Answers.AddRange(answers);
            await _context.SaveChangesAsync();

            return RedirectToAction("MySubmissions");
        }

        // ✅ Show User's Submitted Forms
        public async Task<IActionResult> MySubmissions()
        {
            var user = await _userManager.GetUserAsync(User);
            var forms = await _context.Forms
                .Include(f => f.Template)
                .Where(f => f.UserId == user.Id)
                .ToListAsync();

            return View(forms);
        }

        // ✅ View Filled-Out Form
        public async Task<IActionResult> View(int formId)
        {
            var form = await _context.Forms
                .Include(f => f.Template)
                .Include(f => f.Answers)
                .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null)
                return NotFound();

            return View(form);
        }
    }
}
