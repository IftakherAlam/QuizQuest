using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QuizFormsApp.Models;
using QuizFormsApp.Services;

namespace QuizFormsApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly JiraService _jiraService;

        public TicketController(JiraService jiraService)
        {
            _jiraService = jiraService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
        {
            try
            {
                var jiraLink = await _jiraService.CreateTicketAsync(
                    request.Summary,
                    request.Priority,
                    request.Template,
                    request.PageLink,
                    request.ReporterEmail
                );

                return Ok(new { TicketLink = jiraLink });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
   [HttpGet("my-tickets")]
public async Task<IActionResult> GetMyTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    // 🔍 Print all available claims for debugging
    Console.WriteLine("🔍 User claims:");
    foreach (var claim in User.Claims)
    {
        Console.WriteLine($" - {claim.Type}: {claim.Value}");
    }

    // Try to get the email from claims
    var reporterEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

    if (string.IsNullOrEmpty(reporterEmail))
    {
        Console.WriteLine("❌ User email is missing from claims!");
        return BadRequest(new { Error = "User email not found." });
    }

    Console.WriteLine($"✅ Extracted user email: {reporterEmail}");

    var startAt = (page - 1) * pageSize;

    try
    {
        var tickets = await _jiraService.GetUserTicketsAsync(reporterEmail, startAt, pageSize);
        Console.WriteLine($"✅ Tickets returned from Jira: {tickets.Count}");

        return Ok(new
        {
            Page = page,
            Tickets = tickets
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to get tickets: {ex.Message}");
        return BadRequest(new { Error = ex.Message });
    }
}




    }
}
