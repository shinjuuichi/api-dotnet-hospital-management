namespace WebAPI.Utils;

public class Result<T>
{
    public int Code { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Result(int code, T? data, string message, string status)
    {
        Code = code;
        Data = data;
        Message = message;
        Status = status;
    }
}