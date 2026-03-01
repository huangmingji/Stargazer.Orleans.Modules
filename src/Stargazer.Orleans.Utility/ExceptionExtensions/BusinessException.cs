using System.Collections;
using System.Net;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
    [Serializable]
    public class BusinessException : Exception, IBusinessException, IHasErrorCode, IHasErrorDetails, IHasLogLevel
    {
        public BusinessException(int code, string message = null, string details = null, Exception innerException = null, LogLevel logLevel = LogLevel.Warning)
            :base(message, innerException)
        {
            this.Code = code;
            this.Message = message;
            this.Details = details;
            this.LogLevel = LogLevel;
        }

        public int Code { get; set; }
        public string Details { get; set; }
        public LogLevel LogLevel { get; set; }

        public new string Message { get; set; }

        public BusinessException WithData(string name, object value)
        {
            Data.Add(name, value);
            return this;
        }
    }
}

