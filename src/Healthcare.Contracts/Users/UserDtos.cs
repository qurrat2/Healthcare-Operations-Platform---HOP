using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Users;

public sealed record CreateUserRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password,
    [property: Required, StringLength(150, MinimumLength = 1)] string FullName,
    [property: EmailAddress, StringLength(150)] string? Email,
    [property: Phone, StringLength(30)] string? Phone,
    [property: Required, StringLength(50)] string Role);

public sealed record UpdateUserRequest(
    [property: Required, StringLength(150, MinimumLength = 1)] string FullName,
    [property: EmailAddress, StringLength(150)] string? Email,
    [property: Phone, StringLength(30)] string? Phone,
    bool IsActive);

public sealed record UserResponse(
    long Id,
    string Username,
    string FullName,
    string Role,
    string? Email,
    string? Phone,
    bool IsActive);
