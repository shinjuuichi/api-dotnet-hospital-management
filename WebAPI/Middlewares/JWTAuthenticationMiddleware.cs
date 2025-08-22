using System.Text.Json;
using WebAPI.Utils;

namespace WebAPI.Middleware;

public class JWTAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] AnonymousAllowedPrefixes =
    {
        "/api/v1/auth/register",
        "/api/v1/auth/confirm-otp",
        "/api/v1/auth/login",
        "/api/v1/auth/resend-otp",
        "/api/v1/guest",
        "/swagger",
        "/api-docs"
    };

    public JWTAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (IsAnonymousAllowed(path))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await ReturnUnauthorized(context, "Missing or invalid authorization header");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
        {
            await ReturnUnauthorized(context, "Token is required");
            return;
        }

        var principal = JwtHandler.ValidateToken(token);    
        if (principal == null)
        {
            await ReturnUnauthorized(context, "Invalid or expired token");
            return;
        }

        context.User = principal;
        await _next(context);
    }

    private static bool IsAnonymousAllowed(string path)
    {
        return AnonymousAllowedPrefixes.Any(prefix => path.StartsWith(prefix.ToLower()));
    }

    private static async Task ReturnUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var response = new
        {
            code = 401,
            data = (object?)null,
            message = message,
            status = "fail"
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
