using FymUsers.Api.Dtos;
using FymUsers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FymUsers.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RolesController(AppDbContext db) => _db = db;

    /// <summary>List all roles.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RoleProfile>>> List(CancellationToken ct)
    {
        var roles = await _db.Roles.OrderBy(r => r.Name).ToListAsync(ct);
        return Ok(roles.Select(r => new RoleProfile(r.Id, r.Name, r.Description)).ToList());
    }
}
