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

        // List Templates
        public async Task<IActionResult> Index()
        {
            var templates = await _context.Templates.Include(t => t.Author).ToListAsync();
            return View(templates);
        }

        // Create Template (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Create Template (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Template template)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                template.AuthorId = user.Id;
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }

        // Edit Template (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null || template.AuthorId != _userManager.GetUserId(User))
                return NotFound();

            return View(template);
        }

        // Edit Template (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Template template)
        {
            if (id != template.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }

        // Delete Template
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null || template.AuthorId != _userManager.GetUserId(User))
                return NotFound();

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Search Templates
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

        // Analytics
        public async Task<IActionResult> Analytics(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.Questions) 
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            var analytics = new Dictionary<string, object>();

            foreach (var question in template.Questions.Where(q => q.Type == QuestionType.Integer))  // ✅ FIXED ENUM COMPARISON
            {
                // ✅ Fetch answers correctly from the database
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
