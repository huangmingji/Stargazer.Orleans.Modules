using System.Net;

namespace Stargazer.Orleans.WechatManagement.Silo.Resources;

public class LocalizedException : Exception
{
    public string Code { get; }
    public HttpStatusCode HttpStatusCode { get; }

    public LocalizedException(string code, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError, Exception? innerException = null)
        : base(code, innerException)
    {
        Code = code;
        HttpStatusCode = httpStatusCode;
    }
}
