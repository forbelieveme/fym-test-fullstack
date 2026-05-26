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
    public async Task<ActionResult<List<UserDto>>> List(CancellationToken ct)
    {
        var users = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.UserName)
            .ToListAsync(ct);
        return Ok(users.Select(Map).ToList());
    }

    /// <summary>Get the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub")!);
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw DomainException.NotFound("User");
        return Ok(Map(user));
    }

    /// <summary>Get a user by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw DomainException.NotFound("User");
        return Ok(Map(user));
    }

    /// <summary>Create a new user. Restricted to SuperAdmin.</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.UserName == req.UserName, ct))
            throw DomainException.Conflict("UserName already in use.");
        if (await _db.Users.AnyAsync(u => u.Email == req.Email, ct))
            throw DomainException.Conflict("Email already in use.");

        var user = new User
        {
            UserName = req.UserName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 11),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);

        if (req.RoleIds is { Count: > 0 })
        {
            var validRoleIds = await _db.Roles.Where(r => req.RoleIds!.Contains(r.Id)).Select(r => r.Id).ToListAsync(ct);
            if (validRoleIds.Count != req.RoleIds.Distinct().Count())
                throw DomainException.BadRequest("One or more roles do not exist.");
            foreach (var rid in validRoleIds)
                user.UserRoles.Add(new UserRole { RoleId = rid, AssignedAt = DateTime.UtcNow });
        }

        await _db.SaveChangesAsync(ct);

        var saved = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.Id }, Map(saved));
    }

    /// <summary>Assign one or more roles to a user. SuperAdmin only.</summary>
    [HttpPost("{id:int}/roles")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<UserDto>> AssignRoles(int id, [FromBody] AssignRolesRequest req, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw DomainException.NotFound("User");

        var requested = req.RoleIds.Distinct().ToList();
        var validRoleIds = await _db.Roles.Where(r => requested.Contains(r.Id)).Select(r => r.Id).ToListAsync(ct);
        if (validRoleIds.Count != requested.Count)
            throw DomainException.BadRequest("One or more roles do not exist.");

        foreach (var rid in validRoleIds)
        {
            if (!user.UserRoles.Any(ur => ur.RoleId == rid))
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = rid, AssignedAt = DateTime.UtcNow });
        }
        await _db.SaveChangesAsync(ct);

        var refreshed = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == id, ct);
        return Ok(Map(refreshed));
    }

    private static UserDto Map(User u) => new(
        u.Id, u.UserName, u.Email, u.IsActive, u.CreatedAt,
        u.UserRoles.Select(ur => new RoleDto(ur.Role.Id, ur.Role.Name, ur.Role.Description)).ToList());
}
