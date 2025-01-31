using Microsoft.AspNetCore.Mvc;
using QuizFormsApp.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizFormsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Only keep ONE Index method
        public async Task<IActionResult> Index()
        {
            var topTemplates = await _context.Templates
                .OrderByDescending(t => t.Likes.Count)
                .Take(5)
                .ToListAsync();

            return View(topTemplates);
        }
    }
}
