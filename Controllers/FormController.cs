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

[Authorize(Roles = "Creator")]
public async Task<IActionResult> CreatorSubmissions(int templateId)
{
    var user = await _userManager.GetUserAsync(User);

    var template = await _context.Templates
        .Include(t => t.Forms)
        .ThenInclude(f => f.User)
        .Include(t => t.Forms)
        .ThenInclude(f => f.Answers)
        .ThenInclude(a => a.Question)
        .FirstOrDefaultAsync(t => t.Id == templateId && t.AuthorId == user.Id);

    if (template == null)
    {
        return NotFound();
    }

    return View(template);
}
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> ViewSubmission(int formId)
{
    var user = await _userManager.GetUserAsync(User);
    var form = await _context.Forms
        .Include(f => f.Template)
        .Include(f => f.User)
        .Include(f => f.Answers)
        .ThenInclude(a => a.Question)
        .FirstOrDefaultAsync(f => f.Id == formId);

    if (form == null)
        return NotFound();

    // ✅ Ensure Creators can only view submissions for their own templates
    if (await _userManager.IsInRoleAsync(user, "Creator") && form.Template.AuthorId != user.Id)
    {
        return Forbid();
    }

    return View(form);
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
        Questions = template.Questions
          .OrderBy(q => q.OrderIndex)
          .Select(q => new AnswerViewModel
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
public async Task<IActionResult> Submit(FormViewModel model)
{
    try
    {
        // ✅ Debugging: Check if Model is received correctly
        Console.WriteLine($"🚀 SUBMITTING FORM for Template ID: {model.TemplateId}");
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("❌ ModelState is INVALID!");
            TempData["ErrorMessage"] = "❌ Please correct the errors and try again.";
            return View("Fill", model); // Reload the form with errors
        }

        // ✅ Fetch Template
        var template = await _context.Templates
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == model.TemplateId);

        if (template == null)
        {
            Console.WriteLine("❌ ERROR: Template not found!");
            TempData["ErrorMessage"] = "❌ Template not found.";
            return RedirectToAction("Index");
        }

        // ✅ Fetch User
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            Console.WriteLine("❌ ERROR: User not found!");
            TempData["ErrorMessage"] = "❌ User not found.";
            return RedirectToAction("Index");
        }

        // ✅ Create new Form Submission
        var formSubmission = new Form
        {
            TemplateId = model.TemplateId,
            UserId = user.Id,
            SubmissionDate = DateTime.UtcNow,
            Answers = new List<Answer>()
        };

        Console.WriteLine($"✅ DEBUG: Processing {model.Questions.Count} Answers");

        // ✅ Process and Save Answers
        foreach (var question in model.Questions)
        {
            var answer = new Answer
            {
                QuestionId = question.QuestionId,
                Form = formSubmission
            };

            if (question.Type == "Checkbox")
            {
                answer.BooleanValue = question.CheckboxAnswer;
                Console.WriteLine($"✔ Checkbox Answer: {question.QuestionId} = {answer.BooleanValue}");
            }
            else if (question.Type == "Integer")
            {
                if (int.TryParse(question.Answer, out var intVal))
                {
                    answer.IntegerValue = intVal;
                    Console.WriteLine($"✔ Integer Answer: {question.QuestionId} = {answer.IntegerValue}");
                }
                else
                {
                    Console.WriteLine($"❌ ERROR: Invalid integer for Question ID {question.QuestionId}");
                }
            }
            else
            {
                answer.TextValue = question.Answer;
                Console.WriteLine($"✔ Text Answer: {question.QuestionId} = {answer.TextValue}");
            }

            formSubmission.Answers.Add(answer);
        }

        _context.Forms.Add(formSubmission);
        await _context.SaveChangesAsync();

        Console.WriteLine("✅ FORM SUBMITTED SUCCESSFULLY!");

        TempData["SuccessMessage"] = "✅ Form submitted successfully!";

        // ✅ Redirect to Confirmation Page
        return RedirectToAction("Confirmation", new { id = formSubmission.Id });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR: {ex.Message}");
        TempData["ErrorMessage"] = "❌ An error occurred while submitting the form.";
        return View("Fill", model);
    }
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

[Route("Form/Confirmation/{id}")]
public async Task<IActionResult> Confirmation(int id)
{
    Console.WriteLine($"🚀 LOADING CONFIRMATION PAGE for Form ID: {id}");

    var form = await _context.Forms
        .Include(f => f.Template)
        .Include(f => f.Answers)
        .ThenInclude(a => a.Question)
        .FirstOrDefaultAsync(f => f.Id == id);

    if (form == null)
    {
        Console.WriteLine("❌ ERROR: Form not found!");
        TempData["ErrorMessage"] = "❌ Form submission not found!";
        return RedirectToAction("Index");
    }

    Console.WriteLine("✅ CONFIRMATION PAGE LOADED SUCCESSFULLY!");
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
