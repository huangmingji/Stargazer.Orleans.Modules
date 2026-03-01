using System.Net;
using Stargazer.Orleans.Utility.Extend;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
    [Serializable]
	public class NoNullAllowedException : BusinessException
    {
        public NoNullAllowedException(string message = null, string details = null, Exception innerException = null)
            : base(code: Convert.ToInt32(HttpStatusCode.BadRequest), message: message, details: details, innerException: innerException)
        {
        }

    }
}

