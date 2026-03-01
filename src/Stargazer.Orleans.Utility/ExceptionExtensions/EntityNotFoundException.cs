using System.Net;
using Stargazer.Orleans.Utility.Extend;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
    [Serializable]
	public class EntityNotFoundException : BusinessException
    {
        public EntityNotFoundException(string message = null, string details = null, Exception innerException = null)
            : base(code: Convert.ToInt32(HttpStatusCode.NotFound), message: message, details: details, innerException: innerException)
        {
        }

    }
}

