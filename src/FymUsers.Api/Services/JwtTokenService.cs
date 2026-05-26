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
    private readonly JwtOptions _options;
    public JwtTokenService(JwtOptions options) => _options = options;

    public (string token, DateTime expiresAtUtc) CreateToken(User user, IEnumerable<string> roleNames)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roleNames.Select(roleName => new Claim(ClaimTypes.Role, roleName)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var jwtToken = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwtToken), expiresAtUtc);
    }
}
