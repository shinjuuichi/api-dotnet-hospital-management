namespace WebAPI.Utils.Query;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int Total { get; }
    public int TotalPages { get; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int total)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        Total = total;
        TotalPages = (int)Math.Ceiling((double)total / pageSize);
    }
}
