using FymUsers.Api.Dtos;
using FymUsers.Api.Middleware;
using FymUsers.Api.Services;
using FymUsers.Domain.Entities;
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

    /// <summary>Register a new account. Always assigned the User role.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), 201)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
            throw DomainException.Conflict("UserName already in use.");
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw DomainException.Conflict("Email already in use.");

        var newUser = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        newUser.UserRoles.Add(new UserRole { RoleId = AppDbContext.UserRoleId, AssignedAt = DateTime.UtcNow });
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync(cancellationToken);

        var saved = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == newUser.Id, cancellationToken);

        var roleNames = saved.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (token, expiresAtUtc) = _jwt.CreateToken(saved, roleNames);
        var profile = new UserProfile(
            saved.Id, saved.UserName, saved.Email, saved.IsActive, saved.CreatedAt,
            [.. saved.UserRoles.Select(ur => new RoleProfile(ur.Role.Id, ur.Role.Name, ur.Role.Description))]);

        return Created($"/api/users/{saved.Id}", new LoginResponse(token, expiresAtUtc, profile));
    }
}
