namespace Healthcare.Contracts.Common;

public sealed class PagedResult<T>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public int TotalCount { get; init; }
    public IReadOnlyCollection<T> Items { get; init; } = [];
}
