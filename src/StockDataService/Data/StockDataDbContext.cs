using Microsoft.EntityFrameworkCore;
using StockDataService.Models;

namespace StockDataService.Data
{
    public class StockDataDbContext : DbContext
    {
        public StockDataDbContext(DbContextOptions<StockDataDbContext> options)
            : base(options)

        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StockData>()
                .HasKey(x => new { x.Symbol, x.Date });
        }

        public DbSet<StockData> StockData { get; set; }
    }
}
