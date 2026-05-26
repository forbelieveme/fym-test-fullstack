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
        modelBuilder.Entity<User>(userBuilder =>
        {
            userBuilder.HasKey(user => user.Id);
            userBuilder.Property(user => user.UserName).IsRequired().HasMaxLength(64);
            userBuilder.Property(user => user.Email).IsRequired().HasMaxLength(256);
            userBuilder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(256);
            userBuilder.HasIndex(user => user.UserName).IsUnique();
            userBuilder.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(roleBuilder =>
        {
            roleBuilder.HasKey(role => role.Id);
            roleBuilder.Property(role => role.Name).IsRequired().HasMaxLength(64);
            roleBuilder.Property(role => role.Description).HasMaxLength(256);
            roleBuilder.HasIndex(role => role.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(userRoleBuilder =>
        {
            userRoleBuilder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            userRoleBuilder.HasOne(userRole => userRole.User).WithMany(user => user.UserRoles).HasForeignKey(userRole => userRole.UserId).OnDelete(DeleteBehavior.Cascade);
            userRoleBuilder.HasOne(userRole => userRole.Role).WithMany(role => role.UserRoles).HasForeignKey(userRole => userRole.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = SuperAdminRoleId, Name = RoleNames.SuperAdmin, Description = "Full system access; can create users." },
            new Role { Id = AdminRoleId,      Name = RoleNames.Admin,      Description = "Manages users and roles." },
            new Role { Id = UserRoleId,       Name = RoleNames.User,       Description = "Standard authenticated user." });

        modelBuilder.Entity<User>().Property(user => user.Id).UseIdentityColumn();
        modelBuilder.Entity<Role>().Property(role => role.Id).UseIdentityColumn();
    }
}
