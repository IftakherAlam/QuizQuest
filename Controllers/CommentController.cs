using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizFormsApp.Data;
using QuizFormsApp.Hubs;
using QuizFormsApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentController(AppDbContext context, UserManager<ApplicationUser> userManager, IHubContext<CommentHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Comment comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            comment.UserId = user.Id;
            comment.CreatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Send comment to all connected clients
            await _hubContext.Clients.All.SendAsync("ReceiveComment", comment.TemplateId, user.DisplayName, comment.Text);

            return Ok();
        }
    }
}
