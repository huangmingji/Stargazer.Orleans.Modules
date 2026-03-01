using System.Collections;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
	public interface IBusinessException
	{
		string Message { get; set; }
	}
}

