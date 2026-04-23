namespace Healthcare.Contracts.Auth;

public sealed record CurrentUserResponse(
    long Id,
    string Username,
    string FullName,
    string Role,
    DepartmentSummary? Department);

public sealed record DepartmentSummary(long Id, string Name);
