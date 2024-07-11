using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DDAC_Project.Data;
using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Constants;
using DDAC_Project.Models;
using DDAC_Project;
using Microsoft.AspNetCore.Identity.UI.Services;
using ASPNETCoreIdentityDemo.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DDAC_ProjectContextConnection") ?? throw new InvalidOperationException("Connection string 'DDAC_ProjectContextConnection' not found.");

builder.Services.AddDbContext<DDAC_ProjectContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<DDAC_ProjectUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DDAC_ProjectContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("RequireAdvisorRole", policy => policy.RequireRole(UserRoles.Advisor));
    options.AddPolicy("RequireClientRole", policy => policy.RequireRole(UserRoles.Client));
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<ISenderEmail, EmailSender>();
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DDAC_ProjectUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<DDAC_ProjectContext>();

    // Seed Roles
    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
    await roleManager.CreateAsync(new IdentityRole(UserRoles.Advisor));
    await roleManager.CreateAsync(new IdentityRole(UserRoles.Client));

    // Seed Admin User
    var adminUser = new DDAC_ProjectUser
    {
        FirstName = "Default",
        LastName = "Admin",
        UserName = "admin@fiscella.com",
        Email = "admin@fiscella.com",
        EmailConfirmed = true,
        UserType = "Admin"
    };

    var advisorUser = new DDAC_ProjectUser
    {
        FirstName = "Default",
        LastName = "Advisor",
        UserName = "advisor@fiscella.com",
        Email = "advisor@fiscella.com",
        EmailConfirmed = true,
        UserType = "Advisor"
    };

    if (await userManager.FindByEmailAsync(adminUser.Email) == null)
    {
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
    }

    if (await userManager.FindByEmailAsync(advisorUser.Email) == null)
    {
        var result = await userManager.CreateAsync(advisorUser, "Advisor123!");
        if (result.Succeeded)
        {
            var createdAdvisorUser = await userManager.FindByEmailAsync(advisorUser.Email);
            if (createdAdvisorUser != null)
            {
                var advisor = new Advisor
                {
                    User = createdAdvisorUser,
                    UserId = createdAdvisorUser.Id,
                    YearsOfExperience = "2",
                    Specialization = "Default",
                };

                dbContext.Advisors.Add(advisor);
                await dbContext.SaveChangesAsync();
            }
            await userManager.AddToRoleAsync(advisorUser, UserRoles.Advisor);
        }
    }
    // Seed default categories
        var defaultCategories = new List<Category>
    {
        // Income categories
        new Category { Name = "Salary", Type = "Income", IsDefault = true },
        new Category { Name = "Grants", Type = "Income", IsDefault = true },
        new Category { Name = "Refunds", Type = "Income", IsDefault = true },
        new Category { Name = "Awards", Type = "Income", IsDefault = true },
        new Category { Name = "Sale", Type = "Income", IsDefault = true },

        // Expense categories
        new Category { Name = "Education", Type = "Expense", IsDefault = true },
        new Category { Name = "Entertainment", Type = "Expense", IsDefault = true },
        new Category { Name = "Food", Type = "Expense", IsDefault = true },
        new Category { Name = "Transportation", Type = "Expense", IsDefault = true },
        new Category { Name = "Shopping", Type = "Expense", IsDefault = true },
        new Category { Name = "Health", Type = "Expense", IsDefault = true },
        new Category { Name = "Home", Type = "Expense", IsDefault = true },
        new Category { Name = "Utilities", Type = "Expense", IsDefault = true }
    };

    foreach (var category in defaultCategories)
    {
        if (!await dbContext.Categories.AnyAsync(c => c.Name == category.Name && c.Type == category.Type && c.IsDefault))
        {
            dbContext.Categories.Add(category);
        }
    }

    await dbContext.SaveChangesAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
