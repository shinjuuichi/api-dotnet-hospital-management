using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using WebAPI.Utils;

namespace WebAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new
        {
            code = 0,
            data = (object?)null,
            message = string.Empty,
            status = string.Empty
        };

        switch (exception)
        {
            case DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx:
                var (statusCode, message) = SqlExceptionHandler.HandleSqlException(sqlEx);
                response.StatusCode = statusCode;
                errorResponse = errorResponse with
                {
                    code = statusCode,
                    message = message,
                    status = "fail"
                };
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = errorResponse with
                {
                    code = 400,
                    message = exception.Message,
                    status = "fail"
                };
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = errorResponse with
                {
                    code = 401,
                    message = "Unauthorized access",
                    status = "fail"
                };
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = errorResponse with
                {
                    code = 404,
                    message = "Resource not found",
                    status = "fail"
                };
                break;

            case InvalidOperationException when exception.Message.Contains("conflict") || exception.Message.Contains("duplicate"):
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = errorResponse with
                {
                    code = 409,
                    message = exception.Message,
                    status = "fail"
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = errorResponse with
                {
                    code = 500,
                    message = $"An internal server error occurred: {exception.Message}",
                    status = "error"
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}
