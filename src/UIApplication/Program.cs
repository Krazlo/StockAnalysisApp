using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UIApplication.Data;
using UIApplication.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Identity and Database Configuration ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext for SQL Server - retry pga. SeedData
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));

// Add Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Add role support
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- Existing Service Registrations ---
builder.Services.AddControllersWithViews();

// Add HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

var app = builder.Build();

await SeedData.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // In a real-world scenario, we might add app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Identity Razor Pages
app.MapRazorPages();

// Initialize the database and seed roles/admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Apply pending migrations
        await DbInitializer.Initialize(services); // Seed roles and admin user
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
