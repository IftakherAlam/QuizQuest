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
   [HttpGet]
[Route("Form/Fill/{templateId}")]
public async Task<IActionResult> Fill(int templateId)
{
    Console.WriteLine($"🛠 DEBUG: Fetching Template with ID {templateId}");

    var template = await _context.Templates
        .Include(t => t.Questions)
        .FirstOrDefaultAsync(t => t.Id == templateId);

    if (template == null)
    {
        Console.WriteLine($"❌ ERROR: Template with ID {templateId} not found.");
        return NotFound();
    }

    if (template.Questions == null || !template.Questions.Any())
    {
        Console.WriteLine($"❌ ERROR: Template {templateId} has no questions.");
        return View("NoQuestions"); // ✅ Show a message instead of breaking
    }

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        Console.WriteLine("❌ ERROR: No logged-in user found.");
        return Unauthorized();
    }

    Console.WriteLine($"✅ DEBUG: User {user.Id} is accessing template {templateId}");

    var viewModel = new FormViewModel
    {
        TemplateId = template.Id,
        TemplateTitle = template.Title,
        Questions = template.Questions.Select(q => new AnswerViewModel
        {
            QuestionId = q.Id,
            QuestionText = q.Text,
            Description = q.Description ?? "", // ✅ Ensure Description isn't null
            Type = q.Type.ToString(),
            Answer = ""
        }).ToList()
    };

    Console.WriteLine($"✅ DEBUG: Successfully loaded {viewModel.Questions.Count} questions");

    return View(viewModel);
}


        // ✅ Submit Form

[HttpPost]
[Route("Form/Submit")]
public async Task<IActionResult> Submit(int templateId, Dictionary<int, string> textAnswers, Dictionary<int, int> intAnswers, Dictionary<int, bool> checkboxAnswers)
{
    var template = await _context.Templates
        .Include(t => t.Questions)
        .FirstOrDefaultAsync(t => t.Id == templateId);

    if (template == null)
        return NotFound();

    var user = await _userManager.GetUserAsync(User);

    // ✅ Create a new form submission record
    var form = new Form
    {
        TemplateId = templateId,
        UserId = user.Id,
        SubmissionDate = DateTime.UtcNow
    };

    _context.Forms.Add(form);
    await _context.SaveChangesAsync(); // Save form to get FormId

    var answers = new List<Answer>();

    // ✅ Process Text Answers
    if (textAnswers != null)
    {
        foreach (var entry in textAnswers)
        {
            answers.Add(new Answer { QuestionId = entry.Key, FormId = form.Id, TextValue = entry.Value });
        }
    }

    // ✅ Process Integer Answers
    if (intAnswers != null)
    {
        foreach (var entry in intAnswers)
        {
            answers.Add(new Answer { QuestionId = entry.Key, FormId = form.Id, IntegerValue = entry.Value });
        }
    }

    // ✅ Process Checkbox Answers
    if (checkboxAnswers != null)
    {
        foreach (var entry in checkboxAnswers)
        {
            answers.Add(new Answer { QuestionId = entry.Key, FormId = form.Id, BooleanValue = entry.Value });
        }
    }

    _context.Answers.AddRange(answers);
    await _context.SaveChangesAsync(); // Save all answers

    // ✅ Redirect to Confirmation Page
    return RedirectToAction("Confirmation", new { formId = form.Id });
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


[Route("Form/Confirmation/{formId}")]
public async Task<IActionResult> Confirmation(int formId)
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
