using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Stargazer.Orleans.Utility.ExceptionExtensions;
using Stargazer.Orleans.Utility.Extend;

namespace Stargazer.Orleans.Template.Host.Middlewares;
public class ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, BusinessException ex, IHostEnvironment env)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
        ex.WithData("code", ex.Code);
        ex.WithData("details", ex.Details);
        ex.WithData("logLevel", ex.LogLevel);
        var response = new ResponseMessage()
        {
            Message = ex.Message,
            Data = ex.Data
        };
        var payload = JsonConvert.SerializeObject(response, settings);
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = ex.Code;
        return context.Response.WriteAsync(payload);
    }
}

// 可选：简单扩展方法便于注册
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}