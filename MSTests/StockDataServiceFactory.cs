using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockDataService.Data;
using StockDataService.Services;

public class StockDataServiceFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StockDataDbContext>)
            );

            if (descriptor != null)
                services.Remove(descriptor);

            descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IStockDataService)
            );

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddScoped<IStockDataService, FakeStockDataServiceImpl>();

            // Add In-Memory DB
            services.AddDbContext<StockDataDbContext>(options =>
            {
                options.UseInMemoryDatabase("StockDataTestDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockDataDbContext>();
            db.Database.EnsureCreated();
        });
    }
}