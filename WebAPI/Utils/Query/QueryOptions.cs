namespace WebAPI.Utils.Query;

public record QueryOptions
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortDir { get; init; } = "asc";
    public string? Search { get; init; }
    public Dictionary<string, string>? Filters { get; init; }
}
