using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        // ✅ Redirect Admins to the Admin Panel
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin"); // ✅ Redirect Admins
                }
            }

            var topTemplates = await _context.Templates
                .OrderByDescending(t => t.Likes.Count)
                .Take(5)
                .ToListAsync();

            return View(topTemplates); // ✅ Regular users see normal homepage
        }
         // ✅ View Template Details & Redirect to Form Fill Page
       public async Task<IActionResult> Details(int id)
{
    var template = await _context.Templates
        .Include(t => t.Questions)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (template == null)
    {
        return NotFound();  // ✅ Prevents redirecting with a null template
    }

    return RedirectToAction("Fill", "Form", new { templateId = id });
}
    }
}
