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
    [Authorize(Roles = "Admin")] // ✅ Restrict access to Admins only
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ✅ Admin Dashboard
        public async Task<IActionResult> Index()
        {
            // ✅ Fetch Stats for Dashboard
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalTemplates = await _context.Templates.CountAsync();
            ViewBag.TotalForms = await _context.Forms.CountAsync();  // Assuming `Answers` store form submissions
            
            // ✅ Fetch Recent Templates
            ViewBag.RecentTemplates = await _context.Templates
                .Include(t => t.Author)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            // ✅ User Growth Data (Last 7 Days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .OrderBy(date => date)
                .ToList();

            var userCounts = new List<int>();

            foreach (var day in last7Days)
            {
                int count = await _userManager.Users.CountAsync(u => u.CreatedAt.Date == day);
                userCounts.Add(count);
            }

            ViewBag.Days = last7Days.Select(d => d.ToString("MM/dd"));
            ViewBag.UserCounts = userCounts;

            return View();
        }


        // ✅ View All Templates
public async Task<IActionResult> ManageTemplates()
{
    var templates = await _context.Templates
        .Include(t => t.Author)
        .ToListAsync();

    return View(templates);
}

// ✅ Delete Template
[HttpPost]
public async Task<IActionResult> DeleteTemplate(int id)
{
    var template = await _context.Templates.FindAsync(id);
    if (template == null) return NotFound();

    _context.Templates.Remove(template);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "✅ Template deleted successfully!";
    return RedirectToAction("ManageTemplates");
}

        // ✅ View All Users
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    IsBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > System.DateTimeOffset.UtcNow
                });
            }

            return View(userList);
        }

        // ✅ Assign Admin Role
        [HttpPost]
        public async Task<IActionResult> MakeAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var existingRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, existingRoles); // ✅ Remove all previous roles

                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction("ManageUsers");
        }

        // ✅ Remove Admin Role
        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            return RedirectToAction("ManageUsers");
        }

        // ✅ Block User (Lockout)
        [HttpPost]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
            }
            return RedirectToAction("ManageUsers");
        }

        // ✅ Unblock User (Remove Lockout)
        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return RedirectToAction("ManageUsers");
        }

        // ✅ Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("ManageUsers");
        }

           // ✅ Assign "Creator" Role
        [HttpPost]
        public async Task<IActionResult> MakeCreator(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var existingRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, existingRoles); // ✅ Remove all previous roles

                await _userManager.AddToRoleAsync(user, "Creator");
            }
            return RedirectToAction("ManageUsers");
        }

        // ✅ Remove "Creator" Role
        [HttpPost]
        public async Task<IActionResult> RemoveCreator(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Creator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Creator");
            }
            return RedirectToAction("ManageUsers");
        }

         [HttpPost]
        public async Task<IActionResult> MakeUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var existingRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, existingRoles); // ✅ Remove all previous roles

                await _userManager.AddToRoleAsync(user, "User"); 
            }
            return RedirectToAction("ManageUsers");
        }

        // ✅ Remove "Creator" Role
        [HttpPost]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }
            return RedirectToAction("ManageUsers");
        }

        [Authorize(Roles = "Admin")]
public async Task<IActionResult> AllSubmissions()
{
    var submissions = await _context.Forms
        .Include(f => f.Template)
        .ThenInclude(t => t.Author) // Get Creator details
        .Include(f => f.User) // Get User who submitted
        .Include(f => f.Answers)
        .ThenInclude(a => a.Question)
        .OrderByDescending(f => f.SubmissionDate)
        .ToListAsync();

    return View(submissions);
}
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ViewSubmission(int formId)
{
    var form = await _context.Forms
        .Include(f => f.Template)
        .ThenInclude(t => t.Author) // Include Creator details
        .Include(f => f.User) // Include User who submitted
        .Include(f => f.Answers)
        .ThenInclude(a => a.Question)
        .FirstOrDefaultAsync(f => f.Id == formId);

    if (form == null)
        return NotFound();

    return View(form);
}

    }
}
