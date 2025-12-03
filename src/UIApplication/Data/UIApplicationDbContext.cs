using Microsoft.EntityFrameworkCore;
using UIApplication.Models;

namespace UIApplication.Data
{
    public class UIApplicationDbContext : DbContext
    {
        public UIApplicationDbContext(DbContextOptions<UIApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SavedPrompt>()
                .HasKey(x => x.Id);
        }

        public DbSet<SavedPrompt> SavedPrompts { get; set; }
    }
}
