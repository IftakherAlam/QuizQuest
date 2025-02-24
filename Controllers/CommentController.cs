using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    [Route("Comment")] // Set a base route for consistency
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

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] CommentDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Text) || model.TemplateId <= 0)
                return BadRequest(new { message = "Invalid comment data" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var comment = new Comment
            {
                TemplateId = model.TemplateId,
                UserId = user.Id,
                Text = model.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Send comment to all connected clients via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveComment", model.TemplateId, user.DisplayName, model.Text);

            return Ok(new { success = true });
        }

        [HttpGet("GetByTemplate/{templateId}")]
        public async Task<IActionResult> GetCommentsByTemplate(int templateId)
        {
            var comments = _context.Comments
                .Where(c => c.TemplateId == templateId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Text,
                    c.CreatedAt,
                    UserName = c.User.DisplayName
                }).ToList();

            return Ok(comments);
        }
    }

    public class CommentDto
    {
        public int TemplateId { get; set; }
        public string Text { get; set; }
    }
}
