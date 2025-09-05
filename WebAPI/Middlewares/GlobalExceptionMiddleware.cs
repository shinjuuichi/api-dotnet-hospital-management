using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using WebAPI.Utils;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            status = false
        };

        switch (exception)
        {
            case DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx:
                var (statusCode, message) = SqlExceptionHandler.HandleSqlException(sqlEx);
                response.StatusCode = statusCode;
                errorResponse = errorResponse with
                {
                    code = statusCode,
                    data = (object?)null,
                    message = message,
                    status = false
                };
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = errorResponse with
                {
                    code = 400,
                    data = new { error = exception.Message },
                    message = exception.Message,
                    status = false
                };
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = errorResponse with
                {
                    code = 401,
                    data = new { error = exception.Message },
                    message = "Unauthorized access",
                    status = false
                };
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = errorResponse with
                {
                    code = 404,
                    data = new { error = exception.Message },
                    message = "Resource not found",
                    status = false
                };
                break;

            case InvalidOperationException when exception.Message.Contains("conflict") || exception.Message.Contains("duplicate"):
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = errorResponse with
                {
                    code = 409,
                    data = new { error = exception.Message },
                    message = exception.Message,
                    status = false
                };
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = errorResponse with
                {
                    code = 400,
                    data = new { error = exception.Message },
                    message = exception.Message,
                    status = false
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = errorResponse with
                {
                    code = 500,
                    message = $"An internal server error occurred: {exception.Message}",
                    status = false
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
