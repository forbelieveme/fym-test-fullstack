using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FymUsers.Api.Dtos;
using FymUsers.Api.Middleware;
using FymUsers.Domain.Entities;
using FymUsers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FymUsers.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    /// <summary>List all users with their roles. Requires authentication.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UserProfile>>> List(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken);
        return Ok(users.Select(Map).ToList());
    }

    /// <summary>Get the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfile>> Me(CancellationToken cancellationToken)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var user = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == currentUserId, cancellationToken)
            ?? throw DomainException.NotFound("User");
        return Ok(Map(user));
    }

    /// <summary>Get a user by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfile>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            ?? throw DomainException.NotFound("User");
        return Ok(Map(user));
    }

    /// <summary>Create a new user. Restricted to SuperAdmin.</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<UserProfile>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(user => user.UserName == request.UserName, cancellationToken))
            throw DomainException.Conflict("UserName already in use.");
        if (await _db.Users.AnyAsync(user => user.Email == request.Email, cancellationToken))
            throw DomainException.Conflict("Email already in use.");

        var newUser = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(newUser);

        if (request.RoleIds is { Count: > 0 })
        {
            var validRoleIds = await _db.Roles.Where(role => request.RoleIds.Contains(role.Id)).Select(role => role.Id).ToListAsync(cancellationToken);
            if (validRoleIds.Count != request.RoleIds.Distinct().Count())
                throw DomainException.BadRequest("One or more roles do not exist.");
            foreach (var roleId in validRoleIds)
                newUser.UserRoles.Add(new UserRole { RoleId = roleId, AssignedAt = DateTime.UtcNow });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var savedUser = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .FirstAsync(user => user.Id == newUser.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = savedUser.Id }, Map(savedUser));
    }

    /// <summary>Assign one or more roles to a user. SuperAdmin only.</summary>
    [HttpPost("{id:int}/roles")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<UserProfile>> AssignRoles(int id, [FromBody] AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            ?? throw DomainException.NotFound("User");

        var requestedRoleIds = request.RoleIds.Distinct().ToList();
        var validRoleIds = await _db.Roles.Where(role => requestedRoleIds.Contains(role.Id)).Select(role => role.Id).ToListAsync(cancellationToken);
        if (validRoleIds.Count != requestedRoleIds.Count)
            throw DomainException.BadRequest("One or more roles do not exist.");

        foreach (var roleId in validRoleIds)
        {
            if (!user.UserRoles.Any(userRole => userRole.RoleId == roleId))
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow });
        }
        await _db.SaveChangesAsync(cancellationToken);

        var refreshedUser = await _db.Users
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .FirstAsync(user => user.Id == id, cancellationToken);
        return Ok(Map(refreshedUser));
    }

    private static UserProfile Map(User user) => new(
        user.Id, user.UserName, user.Email, user.IsActive, user.CreatedAt,
        [.. user.UserRoles.Select(userRole => new RoleProfile(userRole.Role.Id, userRole.Role.Name, userRole.Role.Description))]);
}
