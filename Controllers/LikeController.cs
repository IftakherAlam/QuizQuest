using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers
{
    [Authorize]
    [Route("Like")] // Ensures route consistency
    public class LikeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<CommentHub> _hubContext;

        public LikeController(AppDbContext context, UserManager<ApplicationUser> userManager, IHubContext<CommentHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost("ToggleLike")]
        public async Task<IActionResult> ToggleLike(int templateId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existingLike = _context.Likes.FirstOrDefault(l => l.TemplateId == templateId && l.UserId == user.Id);
            if (existingLike == null)
            {
                _context.Likes.Add(new Like { TemplateId = templateId, UserId = user.Id });
            }
            else
            {
                _context.Likes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();
            var likeCount = _context.Likes.Count(l => l.TemplateId == templateId);

            // Broadcast real-time like count update
            await _hubContext.Clients.All.SendAsync("ReceiveLikeUpdate", templateId, likeCount);

            return Ok(new { likes = likeCount });
        }

        [HttpGet("GetLikeCount/{templateId}")]
        public IActionResult GetLikeCount(int templateId)
        {
            var likeCount = _context.Likes.Count(l => l.TemplateId == templateId);
            return Ok(new { likes = likeCount });
        }
    }
}
