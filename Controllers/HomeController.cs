using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using QuizFormsApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace QuizFormsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Home Page - Shows Different Views for Admins and Users
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin"); // ✅ Redirect Admins to the Admin Panel
                }
            }

            // ✅ Fetch Latest 6 Templates for Gallery
            var latestTemplates = await _context.Templates
                .Include(t => t.Author)
                .OrderByDescending(t => t.CreatedAt)
                .Take(6)
                .ToListAsync();

            // ✅ Fetch Top 5 Most Popular Templates (By Submissions)
            var topTemplates = await _context.Templates
                .Include(t => t.Author)
                .Include(t => t.Forms)
                .OrderByDescending(t => t.Forms.Count)
                .Take(5)
                .ToListAsync();

            // ✅ Fetch Top 10 Most Used Tags
            var popularTags = await _context.Tags
                .Include(t => t.TemplateTags)
                .OrderByDescending(t => t.TemplateTags.Count)
                .Take(10)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                LatestTemplates = latestTemplates,
                TopTemplates = topTemplates,
                PopularTags = popularTags
            };

            return View(viewModel); // ✅ Pass Data to View
        }

        // ✅ Language Selector
        [HttpPost]
        public IActionResult SetLanguage(string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return Redirect(Request.Headers["Referer"].ToString()); // Redirect to the previous page
        }

        // ✅ Redirects to the Form Fill Page for Selected Template
        public async Task<IActionResult> Details(int id)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            return RedirectToAction("Fill", "Form", new { templateId = id });
        }
    }
}
