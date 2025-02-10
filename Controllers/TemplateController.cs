using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    public class TemplateController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TemplateController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

 
        // ✅ View All Templates (Everyone can access)
        public async Task<IActionResult> Index()
        {
            var templates = await _context.Templates.Include(t => t.Author).ToListAsync();
            return View(templates);
        }
         // ✅ View Template Details
        public async Task<IActionResult> Details(int id)
        {
            var template = await _context.Templates
                .Include(t => t.Author)
                .Include(t => t.Questions)
                .Include(t => t.AllowedUsers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null) return NotFound();

            // ✅ Check if user has access
            var user = await _userManager.GetUserAsync(User);
            if (!template.IsPublic && !template.AllowedUsers.Any(u => u.UserId == user.Id))
                return Forbid();

            return View(template);
        }
        // ✅ Create Template (Only "Creator" & "Admin" roles can access)
          [Authorize(Roles = "Admin,Creator")]
        public IActionResult Create()
        {
            return View(new Template { Questions = new List<Question>() });
        }

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> Create(Template template)
{
    var user = await _userManager.GetUserAsync(User);
    
    if (user == null)
    {
        return Unauthorized();
    }

    template.AuthorId = user.Id;

    ModelState.Remove("AuthorId");
    ModelState.Remove("Author");

    if (!ModelState.IsValid)
    {
        return View(template);
    }

    _context.Templates.Add(template);
    await _context.SaveChangesAsync();

    // ✅ Redirect to Add Questions page after creating template
    return RedirectToAction("AddQuestions", "Question", new { templateId = template.Id });
}


        // ✅ Edit Template (Only "Creator" can edit their own & Admin can edit any)
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isCreator = await _userManager.IsInRoleAsync(user, "Creator");

            if (isCreator && template.AuthorId != user.Id)
                return Forbid(); // ✅ Prevents Creators from editing others' templates

            return View(template);
        }

     [HttpPost]
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> Edit(int id, Template template)
{
    if (id != template.Id) return NotFound();

    var existingTemplate = await _context.Templates
        .Include(t => t.Questions)  // ✅ Ensure questions are included
        .FirstOrDefaultAsync(t => t.Id == id);

    if (existingTemplate == null) return NotFound();

    var user = await _userManager.GetUserAsync(User);
    bool isCreator = await _userManager.IsInRoleAsync(user, "Creator");

    if (isCreator && existingTemplate.AuthorId != user.Id)
        return Forbid(); // ✅ Prevents Creators from editing others' templates

    if (ModelState.IsValid)
    {
        existingTemplate.Title = template.Title;
        existingTemplate.Description = template.Description;

        // ✅ Update Questions
        foreach (var updatedQuestion in template.Questions)
        {
            var existingQuestion = existingTemplate.Questions.FirstOrDefault(q => q.Id == updatedQuestion.Id);
            if (existingQuestion != null)
            {
                existingQuestion.Text = updatedQuestion.Text;
                existingQuestion.Description = updatedQuestion.Description;
                existingQuestion.Type = updatedQuestion.Type;
                existingQuestion.IsInTable = updatedQuestion.IsInTable;
            }
        }

        _context.Update(existingTemplate);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    return View(template);
}


        // ✅ Delete Template (Only "Creator" can delete their own & Admin can delete any)
        [HttpPost]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isCreator = await _userManager.IsInRoleAsync(user, "Creator");

            if (isCreator && template.AuthorId != user.Id)
                return Forbid(); // ✅ Prevents Creators from deleting others' templates

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✅ Search Templates (Everyone can search)
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
                return View(new List<Template>());

            var results = await _context.Templates
                .Where(t => EF.Functions.ToTsVector("english", t.SearchVector)
                            .Matches(EF.Functions.PlainToTsQuery("english", query)))
                .ToListAsync();

            return View(results);
        }

        // ✅ Analytics (Only Admin can access)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Analytics(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            var analytics = new Dictionary<string, object>();

            foreach (var question in template.Questions.Where(q => q.Type == QuestionType.Integer))
            {
                var values = _context.Answers
                    .Where(a => a.QuestionId == question.Id)
                    .Select(a => int.Parse(a.Value))
                    .ToList();

                if (values.Any())
                {
                    analytics[question.Text] = new
                    {
                        Average = values.Average(),
                        Max = values.Max(),
                        Min = values.Min()
                    };
                }
            }

            return View(analytics);
        }

        
    }
}
