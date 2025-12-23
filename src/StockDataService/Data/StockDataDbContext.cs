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

            modelBuilder.Entity<StockData>(entity =>
            {
                entity.ToTable("StockData");
                entity.HasKey(x => new { x.Symbol, x.Date });

                entity.Property(x => x.Open).HasPrecision(18, 6);
                entity.Property(x => x.High).HasPrecision(18, 6);
                entity.Property(x => x.Low).HasPrecision(18, 6);
                entity.Property(x => x.Close).HasPrecision(18, 6);
            });

       



        }

        public DbSet<StockData> StockData { get; set; }
    }
}
