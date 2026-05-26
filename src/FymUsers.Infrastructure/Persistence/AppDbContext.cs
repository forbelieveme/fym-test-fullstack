using FymUsers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FymUsers.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public const int SuperAdminRoleId = 1;
    public const int AdminRoleId      = 2;
    public const int UserRoleId       = 3;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserName).IsRequired().HasMaxLength(64);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(64);
            e.Property(x => x.Description).HasMaxLength(256);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = SuperAdminRoleId, Name = RoleNames.SuperAdmin, Description = "Full system access; can create users." },
            new Role { Id = AdminRoleId,      Name = RoleNames.Admin,      Description = "Manages users and roles." },
            new Role { Id = UserRoleId,       Name = RoleNames.User,       Description = "Standard authenticated user." });

        modelBuilder.Entity<User>().Property(u => u.Id).UseIdentityColumn();
        modelBuilder.Entity<Role>().Property(r => r.Id).UseIdentityColumn();
    }
}
