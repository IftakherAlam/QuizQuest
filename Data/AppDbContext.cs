using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Models;

namespace QuizFormsApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TemplateUser> TemplateUsers { get; set; }
        public DbSet<Topic> Topics { get; set; } // ✅ Add Topic DbSet

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            

            // ✅ IGNORE 'SearchVector' because EF Core doesn't support 'tsvector'
            builder.Entity<Template>()
                .Ignore(t => t.SearchVector);

            builder.Entity<Topic>().HasData(
                new Topic { Id = 1, Name = "Education" },
                new Topic { Id = 2, Name = "Quiz" },
                new Topic { Id = 3, Name = "Survey" },
                new Topic { Id = 4, Name = "Poll" },
                new Topic { Id = 5, Name = "Other" }
            );
        }
    }
}
