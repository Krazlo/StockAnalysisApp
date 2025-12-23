using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace UIApplication.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties for the user profile here
        public int PromptsUsed { get; set; } = 0;
        public int StocksAnalyzed { get; set; } = 0;
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<StockAnalysis>()
            .HasOne(a => a.UsedPrompt)
            .WithMany()
            .HasForeignKey(a => a.UserPromptId)
            .OnDelete(DeleteBehavior.SetNull);
        }

        public DbSet<UserPrompt> UserPrompts { get; set; }
        public DbSet<StockAnalysis> StockAnalyses { get; set; }
    }
}
