using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DDAC_Project.Data;
using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Constants;

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
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DDAC_ProjectUser>>();

    // Seed Roles
    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
    await roleManager.CreateAsync(new IdentityRole(UserRoles.Advisor));
    await roleManager.CreateAsync(new IdentityRole(UserRoles.Client));

    // Seed Admin User
    var adminUser = new DDAC_ProjectUser
    {
        UserName = "admin@fiscella.com",
        Email = "admin@fiscella.com",
        EmailConfirmed = true
    };

    var advisorUser = new DDAC_ProjectUser
    {
        UserName = "advisor@fiscella.com",
        Email = "advisor@fiscella.com",
        EmailConfirmed = true
    };

    if (await userManager.FindByEmailAsync(adminUser.Email) == null)
    {
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
    }

    if (await userManager.FindByEmailAsync(advisorUser.Email) == null)
    {
        await userManager.CreateAsync(advisorUser, "Advisor123!");
        await userManager.AddToRoleAsync(advisorUser, UserRoles.Advisor);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
