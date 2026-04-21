using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Senparc.Weixin.MP.AdvancedAPIs;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;
using Stargazer.Orleans.WechatManagement.Domain.Messages;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;

namespace Stargazer.Orleans.WechatManagement.Grains.Messages;

[ImplicitStreamSubscription("WechatMessages")]
public class WechatMessageConsumerGrain : IAsyncObserver<StreamMessage>
{
    private readonly IRepository<WechatMessageLog, Guid> _repository;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<WechatMessageConsumerGrain> _logger;

    public WechatMessageConsumerGrain(
        IRepository<WechatMessageLog, Guid> repository,
        IClusterClient clusterClient,
        ILogger<WechatMessageConsumerGrain> logger)
    {
        _repository = repository;
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task OnNextAsync(StreamMessage item, StreamSequenceToken? token = null)
    {
        _logger.LogInformation("Received message: {MessageId}, Type: {MessageType}", item.MessageId, item.MessageType);

        try
        {
            var message = await _repository.FindAsync(item.MessageId);
            if (message == null)
            {
                _logger.LogWarning("Message not found: {MessageId}", item.MessageId);
                return;
            }

            message.Status = WechatMessageStatus.Sending;
            await _repository.UpdateAsync(message);

            var accountGrain = _clusterClient.GetGrain<IWechatAccountGrain>(0);
            var account = await accountGrain.GetAccountAsync(item.AccountId);

            if (account == null)
            {
                _logger.LogWarning("Account not found: {AccountId}", item.AccountId);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, "Account not found");
                return;
            }

            var accessToken = await accountGrain.GetAccessTokenAsync(item.AccountId);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Access token not available for account: {AccountId}", item.AccountId);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, "Access token not available");
                return;
            }

            switch (item.MessageType)
            {
                case WechatMessageType.Template:
                    await SendTemplateMessageAsync(item, account.AppId, accessToken);
                    break;
                case WechatMessageType.Custom:
                    await SendCustomMessageAsync(item, account.AppId, accessToken);
                    break;
                case WechatMessageType.Mass:
                    await SendMassMessageAsync(item, account.AppId, accessToken);
                    break;
                case WechatMessageType.PassiveReply:
                    await SendPassiveReplyAsync(item, account.AppId, accessToken);
                    break;
                default:
                    _logger.LogWarning("Unknown message type: {MessageType}", item.MessageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {MessageId}", item.MessageId);
            await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, ex.Message);
        }
    }

    public Task OnErrorAsync(Exception exception)
    {
        _logger.LogError(exception, "Stream error occurred");
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync()
    {
        _logger.LogInformation("Stream completed");
        return Task.CompletedTask;
    }

    private async Task SendTemplateMessageAsync(StreamMessage item, string appId, string accessToken)
    {
        _logger.LogInformation("Sending template message: {MessageId}", item.MessageId);

        try
        {
            var data = string.IsNullOrEmpty(item.Data)
                ? new Dictionary<string, TemplateMessageDataItem>()
                : JsonSerializer.Deserialize<Dictionary<string, TemplateMessageDataItem>>(item.Data) ?? new();

            var result = await TemplateApi.SendTemplateMessageAsync(accessToken, item.OpenId, item.TemplateId, item.Url, data);

            if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
            {
                _logger.LogInformation("Template message sent successfully: {MessageId}, MsgId: {MsgId}", item.MessageId, result.msgid);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Success, null, result.msgid.ToString());
            }
            else
            {
                _logger.LogError("Failed to send template message: {MessageId}, Error: {Error}", item.MessageId, result.errmsg);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, result.errmsg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template message: {MessageId}", item.MessageId);
            await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, ex.Message);
        }
    }

    private async Task SendCustomMessageAsync(StreamMessage item, string appId, string accessToken)
    {
        _logger.LogInformation("Sending custom message: {MessageId}", item.MessageId);

        try
        {
            var input = string.IsNullOrEmpty(item.Data)
                ? new SendCustomMessageInputDto()
                : JsonSerializer.Deserialize<SendCustomMessageInputDto>(item.Data) ?? new SendCustomMessageInputDto();

            object? result = item.MessageType?.ToLower() switch
            {
                "text" => await CustomApi.SendTextAsync(accessToken, item.OpenId, input.Content ?? ""),
                "image" => await CustomApi.SendImageAsync(accessToken, item.OpenId, input.MediaId ?? ""),
                "voice" => await CustomApi.SendVoiceAsync(accessToken, item.OpenId, input.MediaId ?? ""),
                "video" or "mpvideo" => await CustomApi.SendVideoAsync(accessToken, item.OpenId, input.MediaId ?? "", input.Title, input.Description),
                "music" => await CustomApi.SendMusicAsync(accessToken, item.OpenId, input.MusicUrl ?? "", input.HqMusicUrl ?? "", input.Title, input.Description, input.ThumbMediaId),
                _ => await CustomApi.SendTextAsync(accessToken, item.OpenId, input.Content ?? "")
            };

            _logger.LogInformation("Custom message sent successfully: {MessageId}", item.MessageId);
            await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom message: {MessageId}", item.MessageId);
            await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, ex.Message);
        }
    }

    private async Task SendMassMessageAsync(StreamMessage item, string appId, string accessToken)
    {
        _logger.LogInformation("Sending mass message: {MessageId}", item.MessageId);

        try
        {
            var input = string.IsNullOrEmpty(item.Data)
                ? new SendMassMessageInputDto()
                : JsonSerializer.Deserialize<SendMassMessageInputDto>(item.Data) ?? new SendMassMessageInputDto();

            bool byTag = input.TagId.HasValue && !input.TagId.Value.Equals(Guid.Empty);
            var isText = input.MessageType?.ToLower() == "text";

            string jsonPayload;
            if (byTag)
            {
                var filter = new { tag_id = input.TagId };
                jsonPayload = JsonSerializer.Serialize(new
                {
                    filter = filter,
                    msgtype = isText ? "text" : (input.MessageType?.ToLower() ?? "text"),
                    text = new { content = input.Content ?? "" }
                });
            }
            else if (input.OpenIds?.Count > 0)
            {
                var toUser = string.Join("|", input.OpenIds);
                jsonPayload = JsonSerializer.Serialize(new
                {
                    touser = toUser,
                    msgtype = isText ? "text" : (input.MessageType?.ToLower() ?? "text"),
                    text = new { content = input.Content ?? "" }
                });
            }
            else
            {
                _logger.LogWarning("No recipients specified for mass message: {MessageId}", item.MessageId);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, "No recipients");
                return;
            }

            var apiUrl = "https://api.weixin.qq.com/cgi-bin/message/mass/send?access_token=" + accessToken;
            using var httpClient = new HttpClient();
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<MassSendResult>(responseContent);
            if (result != null && result.errcode == 0)
            {
                _logger.LogInformation("Mass message sent successfully: {MessageId}, MsgId: {MsgId}", item.MessageId, result.msg_id);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Success, null, result.msg_id.ToString());
            }
            else
            {
                var errorMsg = result?.errmsg ?? "Unknown error";
                _logger.LogError("Failed to send mass message: {MessageId}, Error: {Error}", item.MessageId, errorMsg);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, errorMsg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending mass message: {MessageId}", item.MessageId);
            await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, ex.Message);
        }
    }

    private class MassSendResult
    {
        public int errcode { get; set; }
        public string? errmsg { get; set; }
        public long msg_id { get; set; }
        public long msg_data_id { get; set; }
    }

    private async Task SendPassiveReplyAsync(StreamMessage item, string appId, string accessToken)
    {
        _logger.LogInformation("Sending passive reply: {MessageId}, To: {OpenId}", item.MessageId, item.OpenId);

        try
        {
            var content = item.Data ?? "";
            if (string.IsNullOrEmpty(item.OpenId))
            {
                _logger.LogWarning("No receiver specified for passive reply: {MessageId}", item.MessageId);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, "No receiver");
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("No content specified for passive reply: {MessageId}", item.MessageId);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, "No content");
                return;
            }

            var result = await CustomApi.SendTextAsync(accessToken, item.OpenId, content);

            if (result.errcode == Senparc.Weixin.ReturnCode.请求成功)
            {
                _logger.LogInformation("Passive reply sent successfully: {MessageId}", item.MessageId);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Success);
            }
            else
            {
                _logger.LogError("Failed to send passive reply: {MessageId}, Error: {Error}", item.MessageId, result.errmsg);
                await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, result.errmsg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending passive reply: {MessageId}", item.MessageId);
            await UpdateMessageStatusAsync(item.MessageId, WechatMessageStatus.Failed, ex.Message);
        }
    }

    private async Task UpdateMessageStatusAsync(Guid messageId, int status, string? errorMessage = null, string? msgId = null)
    {
        var message = await _repository.FindAsync(messageId);
        if (message != null)
        {
            message.Status = status;
            message.ErrorMessage = errorMessage;
            message.MsgId = msgId;
            if (status == WechatMessageStatus.Success)
            {
                message.CompleteTime = DateTime.UtcNow;
            }
            await _repository.UpdateAsync(message);
        }
    }
}
