using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using System.Linq;
using QuizFormsApp.Models;


namespace QuizFormsApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{apiToken}")]
        public async Task<IActionResult> GetAggregatedResults(string apiToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ApiToken == apiToken);

            if (user == null)
                return Unauthorized("Invalid API token.");

            var templates = await _context.Templates
                .Where(t => t.AuthorId == user.Id)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.Answers)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .ToListAsync();

            var result = templates.Select(t => new
            {
                TemplateTitle = t.Title,
                Author = user.DisplayName,
                Questions = t.Questions.Select(q => new
                {
                    QuestionText = q.Text,
                    QuestionType = q.Type,
                    NumberOfAnswers = q.Answers.Count,
                    AggregatedResult = GetAggregatedResult(q)
                })
            });

            return Ok(result);
        }

    private object GetAggregatedResult(Question question)
    {
        if (question.Type == QuestionType.Integer)
        {
            var numbers = question.Answers
                .Where(a => a.IntegerValue.HasValue)
                .Select(a => (double)a.IntegerValue.Value)
                .ToList();

            return new
            {
                Average = numbers.Any() ? numbers.Average() : 0,
                Min = numbers.Any() ? numbers.Min() : 0,
                Max = numbers.Any() ? numbers.Max() : 0
            };
        }
        else
        {
            return question.Answers
                .GroupBy(a => a.TextValue)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new { Answer = g.Key, Count = g.Count() })
                .ToList();
        }
    }

    }
}
