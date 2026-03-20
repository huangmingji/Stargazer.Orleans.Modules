using System.Net;
using Microsoft.Extensions.Logging;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;

public class EntityNotFoundException(string message = null, string details = null, Exception innerException = null)
    : Exception(message: message, innerException: innerException)
{
    public string Code { get; set; } = nameof(HttpStatusCode.NotFound);
    public string Details { get; set; } = details;
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
}