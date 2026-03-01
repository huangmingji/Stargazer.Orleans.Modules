using Stargazer.Orleans.Utility.Extend;
using NoNullAllowedException = Stargazer.Orleans.Utility.ExceptionExtensions.NoNullAllowedException;

namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
    public class Check
    {
        public static void NotNullOrWhiteSpace(string value, string name)
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new NoNullAllowedException($"{name}不能为空");
            }
        }
    }
}

