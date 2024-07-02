using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DDAC_Project.Data;
using DDAC_Project.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DDAC_ProjectContextConnection") ?? throw new InvalidOperationException("Connection string 'DDAC_ProjectContextConnection' not found.");

builder.Services.AddDbContext<DDAC_ProjectContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<DDAC_ProjectUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DDAC_ProjectContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

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
