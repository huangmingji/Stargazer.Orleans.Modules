using System.Net;
using Stargazer.Orleans.Utility.Extend;

namespace Stargazer.Orleans.Utility.ExceptionExtensions;

[Serializable]
public class VerifyPasswordErrorException : BusinessException
{
    public VerifyPasswordErrorException(string message = null, string details = null, Exception innerException = null)
        : base(code: Convert.ToInt32(HttpStatusCode.Forbidden), message: message, details: details, innerException: innerException)
    {
    }
}