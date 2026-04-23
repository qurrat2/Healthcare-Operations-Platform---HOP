using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Departments;

public sealed record CreateDepartmentRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: StringLength(500)] string? Description);

public sealed record UpdateDepartmentRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: StringLength(500)] string? Description,
    bool IsActive);

public sealed record DepartmentResponse(long Id, string Name, string? Description, bool IsActive);
