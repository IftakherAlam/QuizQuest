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
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Redirect to appropriate dashboard
            if (await _userManager.IsInRoleAsync(user, "Creator"))
                return RedirectToAction("CreatorDashboard");

            return RedirectToAction("UserDashboard");
        }

        // ✅ Creator Dashboard: Shows created templates, likes, comments, and submissions
        [Authorize(Roles = "Creator")]
        public async Task<IActionResult> CreatorDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var templates = await _context.Templates
                .Where(t => t.AuthorId == user.Id)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                .Include(t => t.Forms)
                .ToListAsync();
            return View(templates);
        }

        // ✅ User Dashboard: Shows liked templates, submitted forms, and comments
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var likedTemplates = await _context.Likes
                .Where(l => l.UserId == user.Id)
                .Select(l => l.Template)
                .ToListAsync();

            var submittedForms = await _context.Forms
                .Where(f => f.UserId == user.Id)
                .Include(f => f.Template)
                .ToListAsync();

            var comments = await _context.Comments
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Template)
                .ToListAsync();

            var userDashboardData = new UserDashboardViewModel
            {
                LikedTemplates = likedTemplates,
                SubmittedForms = submittedForms,
                Comments = comments
            };

            return View(userDashboardData);
        }
    }
}
