using Microsoft.EntityFrameworkCore;
using AIAnalysisService.Models;

namespace AIAnalysisService.Data
{
    public class AIAnalysisDbContext : DbContext
    {
        public AIAnalysisDbContext(DbContextOptions<AIAnalysisDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnalysisResponse>()
                .HasKey(x => x.Id);
        }

        public DbSet<AnalysisResponse> AnalysisResponses { get; set; }
    }
}
