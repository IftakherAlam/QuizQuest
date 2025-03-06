using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using QuizFormsApp.Services;
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
        private readonly SalesforceService _salesforceService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context,
            SalesforceService salesforceService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _salesforceService = salesforceService;
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
                Comments = comments,
                IsSalesforceSynced = user.IsSalesforceSynced
            };

            return View(userDashboardData);
        }
         // ✅ Salesforce Sync Action
[HttpPost]
public async Task<IActionResult> SyncToSalesforceForm(SalesforceSyncViewModel model)
{
    var user = await _userManager.GetUserAsync(User);

    var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();

    // Create Account with form data
    var accountId = await _salesforceService.CreateAccountAsync(accessToken, instanceUrl, model.CompanyName);

    // Create Contact linked to Account
    await _salesforceService.CreateContactAsync(
        accessToken,
        instanceUrl,
        user.DisplayName,
        user.Email,
        accountId
    );

    user.IsSalesforceSynced = true;
    await _userManager.UpdateAsync(user);


    TempData["SuccessMessage"] = "Successfully synced with Salesforce!";

    return RedirectToAction("UserDashboard");
}


[HttpGet]
public IActionResult SyncToSalesforceForm()
{
    return View();
}

[HttpGet]
public async Task<IActionResult> ViewMySalesforceData()
{
    var user = await _userManager.GetUserAsync(User);

    var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();
    var contactData = await _salesforceService.GetContactByEmailAsync(accessToken, instanceUrl, user.Email);

    return View(contactData);
}

[Authorize(Roles = "Creator")]
[HttpGet]
public async Task<IActionResult> GetApiToken()
{
    var user = await _userManager.GetUserAsync(User);

    if (string.IsNullOrEmpty(user.ApiToken))
    {
        // Generate token if not existing (optional)
        user.ApiToken = Guid.NewGuid().ToString();
        await _userManager.UpdateAsync(user);
    }

    return Json(new { token = user.ApiToken });
}



    }
}
