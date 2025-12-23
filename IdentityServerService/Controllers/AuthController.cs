using Data.Models;
using Data;
using IdentityServerService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, AppDbContext db, TokenService tokenService)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _userManager.FindByNameAsync(req.Username);
        if (user == null) return Unauthorized();

        if (!await _userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized();

        var scopes = await _db.ApiScopes.Select(s => s.Name).ToArrayAsync();

        var accessToken = _tokenService.CreateAccessToken(user, scopes);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            access_token = accessToken,
            refresh_token = refreshToken.Token
        });
    }
}

public record LoginRequest(string Username, string Password);
