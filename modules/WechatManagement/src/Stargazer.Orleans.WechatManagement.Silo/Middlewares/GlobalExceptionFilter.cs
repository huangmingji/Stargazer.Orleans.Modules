using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions;
using Stargazer.Orleans.WechatManagement.Silo.Resources;

namespace Stargazer.Orleans.WechatManagement.Silo.Middlewares;

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
        var message = localization.GetString(code, language);

        logger.LogWarning(ex, "Exception handled: {Code} ({HttpStatusCode})", code, httpStatusCode);

        context.Result = new ObjectResult(new ResponseData
        {
            Code = code,
            Message = env.IsDevelopment() ? ex.Message : message
        })
        {
            StatusCode = (int)httpStatusCode
        };

        context.ExceptionHandled = true;
    }

    private static string GetCode(Exception ex)
    {
        return ex switch
        {
            LocalizedException localized => localized.Code,
            EntityNotFoundException entityNotFound => entityNotFound.Code,
            ArgumentException => ex.Message,
            UnauthorizedAccessException => "unauthorized",
            InvalidOperationException => ex.Message,
            KeyNotFoundException => "not_found",
            _ => "internal_error"
        };
    }

    private static System.Net.HttpStatusCode GetHttpStatusCode(Exception ex)
    {
        return ex switch
        {
            LocalizedException localized => localized.HttpStatusCode,
            ArgumentException => System.Net.HttpStatusCode.BadRequest,
            UnauthorizedAccessException => System.Net.HttpStatusCode.Unauthorized,
            InvalidOperationException => System.Net.HttpStatusCode.BadRequest,
            KeyNotFoundException => System.Net.HttpStatusCode.NotFound,
            EntityNotFoundException => System.Net.HttpStatusCode.NotFound,
            InvalidCastException => System.Net.HttpStatusCode.Conflict,
            _ => System.Net.HttpStatusCode.InternalServerError
        };
    }
}
