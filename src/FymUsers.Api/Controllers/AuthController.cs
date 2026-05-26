using FymUsers.Api.Dtos;
using FymUsers.Api.Middleware;
using FymUsers.Api.Services;
using FymUsers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FymUsers.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthController(AppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>Log in with username + password and receive a JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == req.UserName, ct);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw DomainException.Unauthorized();

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (token, exp) = _jwt.CreateToken(user, roleNames);

        var dto = new UserDto(
            user.Id, user.UserName, user.Email, user.IsActive, user.CreatedAt,
            user.UserRoles.Select(ur => new RoleDto(ur.Role.Id, ur.Role.Name, ur.Role.Description)).ToList());

        return Ok(new LoginResponse(token, exp, dto));
    }
}
