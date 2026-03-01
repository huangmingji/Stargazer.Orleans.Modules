using Microsoft.Extensions.Logging;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
	public interface IHasLogLevel
	{
        LogLevel LogLevel { get; set; }
    }
}

