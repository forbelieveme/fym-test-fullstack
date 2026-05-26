using System.ComponentModel.DataAnnotations;

namespace FymUsers.Api.Dtos;

public record LoginRequest(
    [Required] string UserName,
    [Required] string Password);

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    UserProfile User);

public record CreateUserRequest(
    [Required, MinLength(3), MaxLength(64)] string UserName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password,
    List<int>? RoleIds);

public record UserProfile(
    int Id,
    string UserName,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    List<RoleProfile> Roles);

public record RoleProfile(
    int Id,
    string Name,
    string? Description);

public record AssignRolesRequest(
    [Required] List<int> RoleIds);
