using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizFormsApp.Models;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.DisplayName = model.DisplayName;
                user.PreferredLanguage = model.PreferredLanguage;
                user.Theme = model.Theme;

                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
            }
            return RedirectToAction("Index");
        }
    }
}
