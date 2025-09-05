namespace WebAPI.DTOs.Common;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool Status { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiResponse : ApiResponse<object>
{
}

public class PagedResponse<T> : ApiResponse<PagedData<T>>
{
}

public class PagedData<T>
{
    public IEnumerable<T> Collection { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
