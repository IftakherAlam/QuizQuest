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

namespace QuizFormsApp.Controllers
{

    public class TemplateController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TemplateController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

 
        // ✅ View All Templates (Everyone can access)
        public async Task<IActionResult> Index()
        {
            var templates = await _context.Templates.Include(t => t.Author).ToListAsync();
            return View(templates);
        }
         // ✅ View Template Details
 public async Task<IActionResult> Details(int id)
{
    var template = await _context.Templates
        .Include(t => t.Author)
        .Include(t => t.Questions)  // ✅ Ensure Questions are included
        .Include(t => t.AllowedUsers)
        .Include(t => t.TemplateTags)
            .ThenInclude(tt => tt.Tag) // ✅ Include related Tag objects
        .Include(t => t.Comments) 
            .ThenInclude(c => c.User) // ✅ Ensure Comments include the User who posted them
        .Include(t => t.Likes)  // ✅ Ensure Likes are included
        .FirstOrDefaultAsync(t => t.Id == id);

    if (template == null)
        return NotFound();

    // ✅ If the template is public, allow access without login
    if (template.IsPublic)
        return View(template);

    // ✅ If the template is NOT public, require login
    if (!User.Identity.IsAuthenticated)
    {
        TempData["ErrorMessage"] = "You must be logged in to access private templates.";
        return RedirectToAction("Login", "Account");
    }

    // ✅ Only allow users with access (Admin always has access)
    var user = await _userManager.GetUserAsync(User);
    if (!template.AllowedUsers.Any(u => u.UserId == user.Id) && !User.IsInRole("Admin"))
    {
        TempData["ErrorMessage"] = "You do not have permission to view this private template.";
        return RedirectToAction("Index", "Home");
    }

    return View(template);
}





        // ✅ Create Template (Only "Creator" & "Admin" roles can access)
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> Create()
{
    var topics = await _context.Topics.ToListAsync();
    
    if (topics == null || !topics.Any())
    {
        TempData["ErrorMessage"] = "No topics found. Please add topics in the database.";
        return RedirectToAction("Index"); // Redirect back to prevent errors
    }

    ViewBag.Topics = new SelectList(topics, "Id", "Name");
    return View(new Template { Questions = new List<Question>() });
}

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> Create(Template template, string selectedUsers, string SelectedTags)
{
    var user = await _userManager.GetUserAsync(User);
    
    
    if (user == null)
    {
        return Unauthorized();
    }
    if (template.TopicId == 0)
    {
        ModelState.AddModelError("TopicId", "Please select a valid topic.");
    }
    // ✅ Ensure model state validation
    ModelState.Remove("AuthorId");
    ModelState.Remove("Author");
    ModelState.Remove("AllowedUsers");
    ModelState.Remove("selectedUsers");
    ModelState.Remove("Topic"); 
    ModelState.Remove("SelectedTags");

    if (!ModelState.IsValid)
    {
        var topics = await _context.Topics.ToListAsync();
        ViewBag.Topics = new SelectList(topics, "Id", "Name", template.TopicId);
        TempData["ErrorMessage"] = "There are validation errors. Please check your input.";
        return View(template);
    }

     template.AuthorId = user.Id;

    // ✅ Save template first so template.Id is generated
    _context.Templates.Add(template);
    await _context.SaveChangesAsync();  // ✅ Now template.Id is available

     if (!string.IsNullOrEmpty(SelectedTags))
    {
        var tagNames = SelectedTags.Split(',').Select(t => t.Trim().ToLower()).Distinct().ToList();

        List<TemplateTag> templateTagsToAdd = new List<TemplateTag>();

        foreach (var tagName in tagNames)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync(); // Save tag first
            }

            // ✅ Ensure duplicate TemplateTag is NOT added
            var existingTemplateTag = await _context.TemplateTags
                .FirstOrDefaultAsync(tt => tt.TemplateId == template.Id && tt.TagId == tag.Id);

            if (existingTemplateTag == null)
            {
                templateTagsToAdd.Add(new TemplateTag
                {
                    TemplateId = template.Id,
                    TagId = tag.Id
                });
            }
        }
        if (templateTagsToAdd.Count > 0)
        {
            _context.TemplateTags.AddRange(templateTagsToAdd); // ✅ Bulk Insert
            await _context.SaveChangesAsync();
        }
    }


    // ✅ Ensure IsPublic is correctly set
    if (template.IsPublic)
    {
        template.AllowedUsers.Clear(); // Remove private users if public
    }
    else
    {
        // ✅ Add Selected Users (for private templates)
        template.AllowedUsers = new List<TemplateUser> 
        { 
            new TemplateUser { UserId = user.Id, TemplateId = template.Id } // ✅ Ensure Creator is always added
        };

        if (!string.IsNullOrEmpty(selectedUsers))
        {
            var emails = selectedUsers.Split(",");
            var users = await _userManager.Users.Where(u => emails.Contains(u.Email)).ToListAsync();

            foreach (var selectedUser in users)
            {
                if (selectedUser.Id != user.Id)
                {
                    template.AllowedUsers.Add(new TemplateUser { UserId = selectedUser.Id, TemplateId = template.Id });
                }
            }
        }
    }

    TempData["SuccessMessage"] = "Template created successfully!";
    return RedirectToAction("AddQuestions", "Question", new { templateId = template.Id });
}


        // ✅ Edit Template (Only "Creator" can edit their own & Admin can edit any)
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> Edit(int id)
{
    var template = await _context.Templates
        .Include(t => t.Questions) // ✅ Ensure questions are included
        .Include(t => t.Topic)
        .Include(t => t.Author)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (template == null)
        return NotFound();

    ViewBag.Topics = await _context.Topics
        .Select(t => new SelectListItem 
        { 
            Value = t.Id.ToString(), 
            Text = t.Name,
            Selected = (t.Id == template.TopicId) // Pre-select current topic
        })
        .ToListAsync();

    var user = await _userManager.GetUserAsync(User);
    bool isCreator = await _userManager.IsInRoleAsync(user, "Creator");

    if (isCreator && template.AuthorId != user.Id)
        return Forbid(); // ✅ Prevents unauthorized access

    Console.WriteLine($"Template ID: {id}, Questions Count: {template.Questions.Count}"); // ✅ Debugging

    return View(template);
}




[HttpPost]
[Authorize(Roles = "Admin,Creator")]
public async Task<IActionResult> Edit(int id, Template template)
{
    if (id != template.Id) return NotFound();

    ModelState.Remove("Name");
    ModelState.Remove("Forms");
    ModelState.Remove("Likes");
    ModelState.Remove("Comments");
    ModelState.Remove("Templates");
    ModelState.Remove("DisplayName");
    ModelState.Remove("Template");
    ModelState.Remove("Author");
    ModelState.Remove("Template");
    ModelState.Remove("Topic");
   

    var existingTemplate = await _context.Templates
        .Include(t => t.Questions)  // ✅ Ensure questions are included
        .Include(t => t.Topic)
        .Include(t => t.Author)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (existingTemplate == null) return NotFound();

    var user = await _userManager.GetUserAsync(User);
    bool isCreator = await _userManager.IsInRoleAsync(user, "Creator");

    if (isCreator && existingTemplate.AuthorId != user.Id)
        return Forbid(); // ✅ Prevents Creators from editing others' templates

        // ✅ Load topics into ViewBag for dropdown


    if (ModelState.IsValid)
    {
        Console.WriteLine($"Template ID: {id}, Title: {existingTemplate.Title}, Description: {existingTemplate.Description}");

        existingTemplate.Title = template.Title;
        existingTemplate.Description = template.Description;
        existingTemplate.ImageUrl = template.ImageUrl;
        existingTemplate.TopicId = template.TopicId;

        template.Author = existingTemplate.Author;
        
        // ✅ Update Questions
        var updatedQuestions = template.Questions ?? new List<Question>();
        foreach (var updatedQuestion in updatedQuestions)
        {
            var existingQuestion = existingTemplate.Questions.FirstOrDefault(q => q.Id == updatedQuestion.Id);
            if (existingQuestion != null)
            {
                existingQuestion.Text = updatedQuestion.Text;
                existingQuestion.Description = updatedQuestion.Description;
                existingQuestion.Type = updatedQuestion.Type;
                 existingQuestion.OrderIndex = updatedQuestion.OrderIndex;
                
                existingQuestion.IsInTable = updatedQuestion.IsInTable;

                _context.Entry(existingQuestion).State = EntityState.Modified;
            }
            else
            {
                updatedQuestion.TemplateId = template.Id;
                _context.Questions.Add(updatedQuestion);
            }
        }

          try
    {
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "✅ Template updated successfully!";
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "❌ Error saving changes: " + ex.Message;
    }

        return RedirectToAction(nameof(Edit), new { id });
    }

     // If validation fails, repopulate ViewBag.Topics
    ViewBag.Topics = await _context.Topics
        .Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.Name,
            Selected = (t.Id == template.TopicId)
        })
        .ToListAsync();

    return View(template);
}


        // ✅ Delete Template (Only "Creator" can delete their own & Admin can delete any)
        [HttpPost]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isCreator = await _userManager.IsInRoleAsync(user, "Creator");

            if (isCreator && template.AuthorId != user.Id)
                return Forbid(); // ✅ Prevents Creators from deleting others' templates

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    
[HttpGet]
public async Task<IActionResult> SearchUsers(string query)
{
    if (string.IsNullOrEmpty(query))
        return BadRequest();

    var users = await _userManager.Users
        .Where(u => u.Email.Contains(query) || u.DisplayName.Contains(query))
        .Select(u => new { u.Email, u.DisplayName })
        .Take(10)
        .ToListAsync();

    return Json(users);
}


        // ✅ Search Templates (Everyone can search)
[HttpGet]
public async Task<IActionResult> Search(string query)
{
    if (string.IsNullOrWhiteSpace(query))
        return View(new List<Template>());

    query = query.Trim();

    var results = await _context.Templates
        .Include(t => t.Author)
        .Include(t => t.Questions)
        .Include(t => t.TemplateTags)
        .ThenInclude(tt => tt.Tag)
        .Where(t =>
            t.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", query)) || // ✅ Full-text search (Title, Description, Questions)
            t.TemplateTags.Any(tt => tt.Tag.Name.Contains(query)) // ✅ Search in Tags
        )
        .ToListAsync();

    return View(results);
}




        // ✅ Analytics (Only Admin can access)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Analytics(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return NotFound();

            var analytics = new Dictionary<string, object>();

            foreach (var question in template.Questions.Where(q => q.Type == QuestionType.Integer))
            {
                var values = _context.Answers
                    .Where(a => a.QuestionId == question.Id)
                    .Select(a => int.Parse(a.Value))
                    .ToList();

                if (values.Any())
                {
                    analytics[question.Text] = new
                    {
                        Average = values.Average(),
                        Max = values.Max(),
                        Min = values.Min()
                    };
                }
            }

            return View(analytics);
        }

        
    }
}
