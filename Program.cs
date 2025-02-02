using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizFormsApp.Data;
using QuizFormsApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Configure Identity (Keep only one instance)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()  
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


//✅ for restrict the admin dashboard to users with the admin role only

builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ✅ Add MVC and Razor Pages
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddRazorPages(); 

var app = builder.Build();

// ✅ Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdmin(services);  
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");  // ❌ This hides detailed errors
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();  // ✅ This shows detailed errors
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // ✅ Ensure this is called only once
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages(); // ✅ Ensure Identity UI Pages are mapped

app.Run();
