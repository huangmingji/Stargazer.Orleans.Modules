using System.Net;
using System.Text.Json;
using Stargazer.Orleans.Users.Grains.Abstractions;

namespace Stargazer.Orleans.Users.Silo.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case ArgumentException argumentException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(ResponseData.Fail(
                    code: "invalid_argument",
                    message: argumentException.Message));
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(ResponseData.Fail(
                    code: "unauthorized",
                    message: "Unauthorized access."));
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(ResponseData.Fail(
                    code: "not_found",
                    message: "Resource not found."));
                break;
            default:
                result = JsonSerializer.Serialize(ResponseData.Fail(
                    code: "internal_server_error",
                    message: "An internal server error occurred."));
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        await context.Response.WriteAsync(result);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
