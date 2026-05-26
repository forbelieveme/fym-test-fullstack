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

    public AuthController(AppDbContext dbContext, IJwtTokenService jwtTokenService)
    {
        _db = dbContext;
        _jwt = jwtTokenService;
    }

    /// <summary>Log in with username + password and receive a JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.UserName == request.UserName, cancellationToken);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw DomainException.Unauthorized();

        var roleNames = user.UserRoles.Select(userRole => userRole.Role.Name).ToList();
        var (token, expiresAtUtc) = _jwt.CreateToken(user, roleNames);

        var userProfile = new UserProfile(
            user.Id, user.UserName, user.Email, user.IsActive, user.CreatedAt,
            [.. user.UserRoles.Select(userRole => new RoleProfile(userRole.Role.Id, userRole.Role.Name, userRole.Role.Description))]);

        return Ok(new LoginResponse(token, expiresAtUtc, userProfile));
    }
}
