using Microsoft.EntityFrameworkCore;
using StockDataService.Data;
using StockDataService.Services;
using Microsoft.EntityFrameworkCore;
using StockDataService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<IStockDataService, StockDataServiceImpl>();
builder.Services.AddScoped<IIndicatorCalculator, IndicatorCalculator>();
builder.Services.AddScoped<IHistoricalDataService, HistoricalDataService>();

// --- DATABASE SETUP ---
builder.Services.AddDbContext<StockDataDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
{ }


// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthorization();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StockDataDbContext>();

    try
    {
        db.Database.Migrate();
    }
    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1801)
    {
        // Database already exists ? ignore
    }
}

app.MapControllers();

app.Run();
