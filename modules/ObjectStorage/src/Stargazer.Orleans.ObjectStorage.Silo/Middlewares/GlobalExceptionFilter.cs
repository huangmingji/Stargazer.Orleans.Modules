using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Silo.Resources;

namespace Stargazer.Orleans.ObjectStorage.Silo.Middlewares;

public class GlobalExceptionFilter(
    IHostEnvironment env,
    LocalizationService localization,
    ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var code = GetCode(ex);
        var httpStatusCode = GetHttpStatusCode(ex);
        var language = localization.GetCurrentLanguage(context.HttpContext);
        var message = localization.GetMessage(code, language);

        if (httpStatusCode >= 500)
        {
            logger.LogError(ex, "Unhandled exception: {Code} - {Message}", code, message);
        }
        else
        {
            logger.LogWarning("Exception: {Code} - {Message}", code, message);
        }

        var response = new ResponseData
        {
            Code = code,
            Message = message,
            Data = env.IsDevelopment() ? new { error = ex.Message, stackTrace = ex.StackTrace } : null
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = httpStatusCode,
            DeclaredType = typeof(ResponseData)
        };
        context.ExceptionHandled = true;
    }

    private static string GetCode(Exception ex)
    {
        if (ex is LocalizedException localized)
        {
            return localized.Code;
        }
        return ex.Message;
    }

    private static int GetHttpStatusCode(Exception ex)
    {
        if (ex is LocalizedException localized)
        {
            return localized.HttpStatusCode;
        }
        return ex switch
        {
            ArgumentException => 400,
            UnauthorizedAccessException => 401,
            InvalidOperationException => 400,
            KeyNotFoundException => 404,
            EntityNotFoundException => 404,
            InvalidCastException => 409,
            _ => 500
        };
    }
}

public static class GlobalExceptionFilterExtensions
{
    public static IMvcBuilder AddGlobalExceptionFilter(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options => options.Filters.Add<GlobalExceptionFilter>());
        return builder;
    }
}
