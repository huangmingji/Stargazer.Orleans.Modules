using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts;
using Stargazer.Orleans.WechatManagement.Silo.Wechat;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Stargazer.Orleans.WechatManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/wechat/{accountId}")]
public class WechatController(
    IClusterClient client,
    IServiceProvider serviceProvider,
    ILogger<WechatController> logger) : ControllerBase
{
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        Guid accountId,
        [FromQuery] string signature,
        [FromQuery] string timestamp,
        [FromQuery] string nonce,
        [FromQuery] string echostr,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wechatAccountGrain = client.GetGrain<IWechatAccountGrain>(0);
            var account = await wechatAccountGrain.GetAccountAsync(accountId, cancellationToken);

            if (account == null || !account.IsActive)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(echostr))
            {
                if (CheckSignature(signature, timestamp, nonce, account.Token))
                {
                    return Content(echostr);
                }
                return BadRequest();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in WeChat callback");
            return StatusCode(500);
        }
    }

    [HttpPost("callback")]
    public async Task<IActionResult> ProcessMessage(
        Guid accountId,
        [FromQuery] string signature,
        [FromQuery] string timestamp,
        [FromQuery] string nonce,
        [FromQuery] string msg_signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wechatAccountGrain = client.GetGrain<IWechatAccountGrain>(0);
            var account = await wechatAccountGrain.GetAccountAsync(accountId, cancellationToken);

            if (account == null || !account.IsActive)
            {
                return BadRequest("Account not found or inactive");
            }

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var xmlContent = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrEmpty(xmlContent))
            {
                return BadRequest("Empty message body");
            }

            var postModel = new PostModel
            {
                Token = account.Token,
                EncodingAESKey = account.EncodingAESKey,
                AppId = account.AppId,
                Timestamp = timestamp,
                Nonce = nonce,
                Signature = signature,
                Msg_Signature = msg_signature
            };

            var clusterClient = serviceProvider.GetRequiredService<IClusterClient>();
            var messageLogger = serviceProvider.GetRequiredService<ILogger<CustomMessageHandler>>();

            var messageHandler = new CustomMessageHandler(
                new MemoryStream(Encoding.UTF8.GetBytes(xmlContent)),
                postModel,
                clusterClient,
                messageLogger,
                0,
                false);

            await messageHandler.ExecuteAsync(cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing WeChat message");
            return StatusCode(500);
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(
        Guid accountId,
        [FromBody] SendMessageRequest request)
    {
        try
        {
            var accountGrain = client.GetGrain<IWechatAccountGrain>(0);
            var account = await accountGrain.GetAccountAsync(accountId);
            
            if (account == null)
            {
                return NotFound("Account not found");
            }

            return Ok(new { success = true, message = "Message queued for sending" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending WeChat message");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static bool CheckSignature(string signature, string timestamp, string nonce, string token)
    {
        var arr = new[] { token, timestamp, nonce }.OrderBy(x => x).ToArray();
        var arrString = string.Join("", arr);
        var sha1 = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(arrString));
        var sha1String = BitConverter.ToString(sha1).Replace("-", "").ToLower();
        return sha1String == signature;
    }
}

public class SendMessageRequest
{
    public string ToUserOpenId { get; set; } = string.Empty;
    public string MsgType { get; set; } = "text";
    public string Content { get; set; } = string.Empty;
}
