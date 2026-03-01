using System.Net;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
	public interface IHasErrorCode
	{
		int Code { get; set; }
	}
}

