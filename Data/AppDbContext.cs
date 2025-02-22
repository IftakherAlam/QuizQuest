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
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TemplateTag> TemplateTags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            

            // ✅ IGNORE 'SearchVector' because EF Core doesn't support 'tsvector'
             builder.Entity<Template>(entity =>
        {
            // Configure the computed column for full-text search
            entity.Property(t => t.SearchVector)
                .HasComputedColumnSql(
                    "to_tsvector('english', coalesce(\"Title\", '') || ' ' || coalesce(\"Description\", ''))",
                    stored: true);

            // Create a GIN index on the SearchVector
            entity.HasIndex(t => t.SearchVector)
                .HasMethod("GIN");
        });



            builder.Entity<TemplateTag>()
                .HasKey(tt => new { tt.TemplateId, tt.TagId });

            builder.Entity<TemplateTag>()
                .HasOne(tt => tt.Template)
                .WithMany(t => t.TemplateTags)
                .HasForeignKey(tt => tt.TemplateId);

            builder.Entity<TemplateTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TemplateTags)
                .HasForeignKey(tt => tt.TagId);

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
