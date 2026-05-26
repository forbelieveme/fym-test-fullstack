using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FymUsers.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FymUsers.Api.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SigningKey { get; set; } = null!;
    public int ExpiryMinutes { get; set; } = 60;
}

public interface IJwtTokenService
{
    (string token, DateTime expiresAtUtc) CreateToken(User user, IEnumerable<string> roleNames);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;
    public JwtTokenService(JwtOptions opt) => _opt = opt;

    public (string token, DateTime expiresAtUtc) CreateToken(User user, IEnumerable<string> roleNames)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roleNames.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_opt.ExpiryMinutes);

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
