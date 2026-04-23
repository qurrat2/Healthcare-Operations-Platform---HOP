namespace Healthcare.Contracts.Common;

public record PaginationRequest(
    int Page = 1,
    int Limit = 20,
    string? SortBy = null,
    string? SortOrder = null);
