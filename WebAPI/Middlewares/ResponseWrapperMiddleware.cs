using System.Text.Json;
using WebAPI.DTOs.Common;

namespace WebAPI.Middlewares;

public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("swagger") || path.Contains("api-docs") || path.Contains("images"))
        {
            await _next(context);
            return;
        }

        var originalResponseBody = context.Response.Body;
        
        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            if (context.Response.HasStarted)
            {
                return;
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            
            if (ShouldWrapResponse(context, responseBody))
            {
                var wrappedResponse = WrapResponse(context, responseBody);
                var wrappedJson = JsonSerializer.Serialize(wrappedResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                context.Response.Body = originalResponseBody;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength = null;
                await context.Response.WriteAsync(wrappedJson);
            }
            else
            {
                context.Response.Body = originalResponseBody;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalResponseBody);
            }
        }
        catch (Exception)
        {
            context.Response.Body = originalResponseBody;
            throw;
        }
    }

    private static bool ShouldWrapResponse(HttpContext context, string responseBody)
    {
        if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 300)
            return false;
            
        if (string.IsNullOrWhiteSpace(responseBody))
            return false;
            
        var contentType = context.Response.ContentType;
        if (!string.IsNullOrEmpty(contentType) && !contentType.Contains("application/json"))
            return false;
            
        try
        {
            if (responseBody.TrimStart().StartsWith("{") && 
                responseBody.Contains("\"code\"") && 
                responseBody.Contains("\"status\""))
                return false;
        }
        catch
        {
            // If we can't parse, continue to wrap
        }
            
        return true;
    }

    private static object WrapResponse(HttpContext context, string responseBody)
    {
        object? data = null;
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                data = JsonSerializer.Deserialize<object>(responseBody);
            }
            catch
            {
                data = responseBody;
            }
        }

        context.Response.StatusCode = 200;

        return new
        {
            code = 200,
            status = true,
            data = data,
            message = "OK"
        };
    }
}
