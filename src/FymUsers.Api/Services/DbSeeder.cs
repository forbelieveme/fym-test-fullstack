using FymUsers.Domain.Entities;
using FymUsers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FymUsers.Api.Services;

public static class DbSeeder
{
    // For a real system the seed password would come from a secret store; here we use a fixed
    // default so the reviewer can log in immediately after `dotnet ef database update`.
    public const string SeedSuperAdminUserName = "superadmin";
    public const string SeedSuperAdminPassword = "SuperAdmin123!";
    public const string SeedSuperAdminEmail    = "superadmin@fym.local";

    public static async Task EnsureSuperAdminAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (await db.Users.AnyAsync(u => u.UserName == SeedSuperAdminUserName, ct))
            return;

        var user = new User
        {
            UserName = SeedSuperAdminUserName,
            Email = SeedSuperAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedSuperAdminPassword, workFactor: 11),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { RoleId = AppDbContext.SuperAdminRoleId, AssignedAt = DateTime.UtcNow });
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }
}
