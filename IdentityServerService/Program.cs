using Data;
using Data.Models;
using IdentityServerService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Supabase;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);


// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// RSA signing key
RSA rsa = RSA.Create();
var key = new RsaSecurityKey(rsa);

// JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = key,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

// Token Service
builder.Services.AddSingleton(new TokenService(key));

// SUPABASE
//var supaUrl = builder.Configuration["Supabase:Url"];
//var supaKey = builder.Configuration["Supabase:Key"];

//var supabaseOptions = new SupabaseOptions
//{
//    AutoConnectRealtime = false,
//    AutoRefreshToken = true
//};

//// Opret og initialiser klienten asynkront
//var supabaseClient = new Supabase.Client(supaUrl, supaKey, supabaseOptions);
//await supabaseClient.InitializeAsync(); // sørger for at auth/session info er klar


//builder.Services.AddSingleton(supabaseClient);
// Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
