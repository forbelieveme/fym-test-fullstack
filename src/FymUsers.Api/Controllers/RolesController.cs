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
    public RolesController(AppDbContext dbContext) => _db = dbContext;

    /// <summary>List all roles.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RoleProfile>>> List(CancellationToken cancellationToken)
    {
        var roles = await _db.Roles.OrderBy(role => role.Name).ToListAsync(cancellationToken);
        return Ok(roles.Select(role => new RoleProfile(role.Id, role.Name, role.Description)).ToList());
    }
}
