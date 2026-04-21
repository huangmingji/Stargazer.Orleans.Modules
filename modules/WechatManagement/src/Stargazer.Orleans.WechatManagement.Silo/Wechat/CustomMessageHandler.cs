using Microsoft.Extensions.Logging;
using Orleans;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageContexts;
using Senparc.Weixin.MP.MessageHandlers;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

namespace Stargazer.Orleans.WechatManagement.Silo.Wechat;

public class CustomMessageHandler(
    Stream inputStream,
    PostModel postModel,
    IClusterClient clusterClient,
    ILogger<CustomMessageHandler> logger,
    int maxRecordCount = 0,
    bool onlyAllowEncryptMessage = false)
    : MessageHandler<DefaultMpMessageContext>(inputStream, postModel, maxRecordCount, onlyAllowEncryptMessage, null)
{
    private readonly IClusterClient _clusterClient = clusterClient;
    private readonly ILogger<CustomMessageHandler> _logger = logger;

    public override async Task<IResponseMessageBase> OnEventRequestAsync(IRequestMessageEventBase requestMessage)
    {
        var eventType = requestMessage.Event.ToString();

        if (eventType == "subscribe")
        {
            return await HandleSubscribeAsync(requestMessage);
        }
        else if (eventType == "unsubscribe")
        {
            return await HandleUnsubscribeAsync(requestMessage);
        }

        return await base.OnEventRequestAsync(requestMessage);
    }

    private async Task<IResponseMessageBase> HandleSubscribeAsync(IRequestMessageEventBase requestMessage)
    {
        var openId = requestMessage.FromUserName;
        var accountOpenId = requestMessage.ToUserName;

        _logger.LogInformation("User {OpenId} subscribed to account {AccountOpenId}", openId, accountOpenId);

        try
        {
            var accountGrain = _clusterClient.GetGrain<IWechatAccountGrain>(0);
            var accounts = await accountGrain.GetAllAccountsAsync(default);

            var account = accounts.FirstOrDefault(a => a.Name == accountOpenId);
            if (account == null)
            {
                _logger.LogWarning("Account {AccountOpenId} not found", accountOpenId);
                return SuccessResponseMessage(requestMessage, "公众号未配置");
            }

            var userGrain = _clusterClient.GetGrain<IWechatUserGrain>(0);

            try
            {
                var userInfo = Senparc.Weixin.MP.AdvancedAPIs.UserApi.Info(
                    account.AppId,
                    openId);

                if (userInfo?.errcode == Senparc.Weixin.ReturnCode.请求成功 && userInfo != null)
                {
                    await userGrain.SaveUserInfoAsync(
                        account.Id,
                        openId,
                        userInfo.unionid,
                        userInfo.nickname,
                        userInfo.sex,
                        userInfo.province,
                        userInfo.city,
                        userInfo.country,
                        userInfo.headimgurl,
                        default);
                }
                else
                {
                    await userGrain.SaveUserInfoAsync(
                        account.Id,
                        openId,
                        null,
                        "微信用户",
                        0,
                        null,
                        null,
                        null,
                        null,
                        default);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user info from WeChat API, saving with default values");
                await userGrain.SaveUserInfoAsync(
                    account.Id,
                    openId,
                    null,
                    "微信用户",
                    0,
                    null,
                    null,
                    null,
                    null,
                    default);
            }

            _logger.LogInformation("User {OpenId} saved successfully", openId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user {OpenId}", openId);
        }

        return SuccessResponseMessage(requestMessage, "欢迎关注！");
    }

    private async Task<IResponseMessageBase> HandleUnsubscribeAsync(IRequestMessageEventBase requestMessage)
    {
        var openId = requestMessage.FromUserName;
        var accountOpenId = requestMessage.ToUserName;

        _logger.LogInformation("User {OpenId} unsubscribed from account {AccountOpenId}", openId, accountOpenId);

        try
        {
            var accountGrain = _clusterClient.GetGrain<IWechatAccountGrain>(0);
            var accounts = await accountGrain.GetAllAccountsAsync(default);

            var account = accounts.FirstOrDefault(a => a.Name == accountOpenId);
            if (account == null)
            {
                return null;
            }

            var userGrain = _clusterClient.GetGrain<IWechatUserGrain>(0);
            var user = await userGrain.GetUserByOpenIdAsync(account.Id, openId, default);

            if (user != null)
            {
                await userGrain.UnSubscribeAsync(user.Id, default);
                _logger.LogInformation("User {OpenId} unsubscribed successfully", openId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling unsubscribe for {OpenId}", openId);
        }

        return null;
    }

    public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
    {
        return new ResponseMessageText
        {
            Content = "收到消息",
            CreateTime = DateTime.Now,
            FromUserName = requestMessage.ToUserName,
            ToUserName = requestMessage.FromUserName
        };
    }

    private static ResponseMessageText SuccessResponseMessage(IRequestMessageBase requestMessage, string content)
    {
        return new ResponseMessageText
        {
            Content = content,
            CreateTime = DateTime.Now,
            FromUserName = requestMessage.ToUserName,
            ToUserName = requestMessage.FromUserName
        };
    }
}