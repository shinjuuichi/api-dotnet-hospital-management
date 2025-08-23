using System.Text.Json;

namespace WebAPI.Middleware;

public class ResponseWrappingMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        var response = context.Response;
        response.Body = originalBodyStream;

        if (ShouldWrapResponse(context))
        {
            await WrapResponse(context, responseBody);
        }
        else
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private static bool ShouldWrapResponse(HttpContext context)
    {
        if (context.Response.HasStarted)
            return false;

        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/swagger") || path.Contains("/api-docs") ||
            path.Contains(".js") || path.Contains(".css") ||
            path.Contains(".html") || path.Contains(".ico"))
            return false;

        return context.Request.Path.StartsWithSegments("/api");
    }

    private static async Task WrapResponse(HttpContext context, MemoryStream responseBody)
    {
        var response = context.Response;
        responseBody.Seek(0, SeekOrigin.Begin);

        object? data = null;
        if (responseBody.Length > 0)
        {
            var content = await new StreamReader(responseBody).ReadToEndAsync();
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    data = JsonSerializer.Deserialize<object>(content);
                }
                catch
                {
                    data = content;
                }
            }
        }

        var wrappedResponse = new
        {
            code = response.StatusCode,
            data = data,
            message = GetStatusMessage(response.StatusCode),
            status = GetStatus(response.StatusCode)
        };

        var jsonResponse = JsonSerializer.Serialize(wrappedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.ContentType = "application/json";
        response.ContentLength = null;
        await response.WriteAsync(jsonResponse);
    }

    private static string GetStatusMessage(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "OK",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            >= 400 and < 500 => "Client Error",
            >= 500 => "Internal Server Error",
            _ => "Unknown"
        };
    }

    private static string GetStatus(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "success",
            >= 400 and < 500 => "fail",
            >= 500 => "error",
            _ => "unknown"
        };
    }
}