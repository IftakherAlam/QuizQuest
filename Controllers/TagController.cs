using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizFormsApp.Controllers{
[Route("Tag")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly AppDbContext _context;

    public TagController(AppDbContext context)
    {
        _context = context;
    }

[HttpGet("GetTags")]
public async Task<IActionResult> GetTags(string query)
{
    if (string.IsNullOrEmpty(query))
    {
        return Ok(new List<object>());
    }

    var tags = await _context.Tags
        .Where(t => t.Name.ToLower().Contains(query.ToLower()))
        .Select(t => new { id = t.Id, name = t.Name })
        .Take(5)  // Limit results to 5 suggestions
        .ToListAsync();

    return Ok(tags);
}
}
}