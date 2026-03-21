using System.Net;
using Microsoft.Extensions.Logging;

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;

public class EntityNotFoundException : Exception
{
    public string Code { get; set; } = nameof(HttpStatusCode.NotFound);
    public string Details { get; set; }
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;

    public EntityNotFoundException(string? message = null, string? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Details = details ?? string.Empty;
    }
}