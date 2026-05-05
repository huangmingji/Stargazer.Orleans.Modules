namespace Stargazer.Orleans.MessageManagement.Silo.Resources;

public class LocalizedException : Exception
{
    public string Code { get; }
    public int HttpStatusCode { get; }

    public LocalizedException(string code, int httpStatusCode = 500, string? message = null)
        : base(message ?? code)
    {
        Code = code;
        HttpStatusCode = httpStatusCode;
    }

    public static LocalizedException BadRequest(string code, string? message = null)
        => new(code, 400, message);

    public static LocalizedException Unauthorized(string code, string? message = null)
        => new(code, 401, message);

    public static LocalizedException Forbidden(string code, string? message = null)
        => new(code, 403, message);

    public static LocalizedException NotFound(string code, string? message = null)
        => new(code, 404, message);

    public static LocalizedException Conflict(string code, string? message = null)
        => new(code, 409, message);
}
