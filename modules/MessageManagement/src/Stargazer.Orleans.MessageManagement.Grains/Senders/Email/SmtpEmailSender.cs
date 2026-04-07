using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Email;

/// <summary>
/// 基于 SMTP 协议的邮件发送器（使用 MailKit）
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public string ProviderName => "smtp";

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _settings = configuration.GetSection("Message:Email:Smtp").Get<SmtpSettings>() ?? new SmtpSettings();
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(_settings.From, to, subject, body, cancellationToken);
    }

    public async Task<EmailSendResult> SendAsync(
        string from,
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            if (body.Contains("<"))
            {
                message.Body = new TextPart("html") { Text = body };
            }
            else
            {
                message.Body = new TextPart("plain") { Text = body };
            }

            using var client = new SmtpClient();
            
            var secureSocketOptions = _settings.UseSsl 
                ? SecureSocketOptions.StartTls 
                : SecureSocketOptions.None;
                
            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            var messageId = await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}, MessageId: {MessageId}", to, messageId);

            return new EmailSendResult { Success = true, MessageId = messageId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {To}", to);
            return new EmailSendResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<List<EmailSendResult>> BatchSendAsync(
        List<EmailSendRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var results = new List<EmailSendResult>();
        
        foreach (var request in requests)
        {
            var result = await SendAsync(
                request.From ?? _settings.From, 
                request.To, 
                request.Subject, 
                request.Body, 
                cancellationToken);
            results.Add(result);
        }
        
        return results;
    }
}
