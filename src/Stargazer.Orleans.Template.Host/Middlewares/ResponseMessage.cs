using System.Collections;

namespace Stargazer.Orleans.Template.Host.Middlewares;

public class ResponseMessage
{
    public string Message { get; set; }
    public IDictionary Data { get; set; }
}