using Data.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityServerService.Services;

public class TokenService
{
    private readonly RsaSecurityKey _key;

    public TokenService(RsaSecurityKey key)
    {
        _key = key;
    }

    public string CreateAccessToken(ApplicationUser user, string[] scopes)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim("name", user.UserName)
        };

        claims.AddRange(scopes.Select(s => new Claim("scope", s)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
