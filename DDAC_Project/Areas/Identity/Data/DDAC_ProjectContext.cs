using DDAC_Project.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DDAC_Project.Models;

namespace DDAC_Project.Data;

public class DDAC_ProjectContext : IdentityDbContext<DDAC_ProjectUser>
{
    public DDAC_ProjectContext(DbContextOptions<DDAC_ProjectContext> options)
        : base(options)
    {
    }

    public DbSet<Advisor> Advisors { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Entity<Advisor>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Client>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Client>()
            .HasOne(c => c.Advisor)
            .WithMany()
            .HasForeignKey(c => c.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Budget>()
            .HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Category>()
            .HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Goal>()
            .HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(b => b.Advisor)
            .WithMany()
            .HasForeignKey(b => b.AdvisorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Transaction>()
            .HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Transaction>()
            .HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Transaction>()
            .HasOne(b => b.Goal)
            .WithMany()
            .HasForeignKey(b => b.GoalId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
