using System.Text.Json;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.WechatManagement.Domain;
using Stargazer.Orleans.WechatManagement.Domain.Users;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

namespace Stargazer.Orleans.WechatManagement.Grains;

public class WechatLoginGrain(
    IRepository<WechatUserBinding, Guid> bindingRepository,
    IRepository<WechatUser, Guid> wechatUserRepository)
    : Grain, IWechatLoginGrain
{
    public async Task<string> GenerateQrCodeAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var token = new SequentialGuid().Create().ToString();

        var qrCodeUrl = $"https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token=";
        
        return JsonSerializer.Serialize(new
        {
            token = token,
            scene_id = token,
            expire_seconds = 300,
            action_info = new
            {
                button = new
                {
                    name = "绑定账号"
                }
            }
        });
    }

    public async Task<(bool Success, string? Token, string? Message)> ProcessScanResultAsync(
        Guid accountId,
        string openId,
        string sceneId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wechatUsers = await wechatUserRepository.FindListAsync(
                u => u.AccountId == accountId && u.OpenId == openId,
                cancellationToken);

            var wechatUser = wechatUsers.FirstOrDefault();
            if (wechatUser == null)
            {
                return (false, null, "微信用户不存在");
            }

            var binding = await bindingRepository.FindListAsync(
                b => b.WechatUserId == wechatUser.Id && b.AccountId == accountId && b.IsActive,
                cancellationToken);

            var existingBinding = binding.FirstOrDefault();

            if (existingBinding != null)
            {
                return (true, existingBinding.LocalUserId.ToString(), "登录成功，请使用本地账号登录接口获取Token");
            }
            else
            {
                return (false, null, "需要绑定本地账号");
            }
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Token, string? Message)> BindLocalUserAsync(
        Guid accountId,
        string openId,
        Guid localUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wechatUsers = await wechatUserRepository.FindListAsync(
                u => u.AccountId == accountId && u.OpenId == openId,
                cancellationToken);

            var wechatUser = wechatUsers.FirstOrDefault();
            if (wechatUser == null)
            {
                return (false, null, "微信用户不存在");
            }

            var existingBindings = await bindingRepository.FindListAsync(
                b => b.WechatUserId == wechatUser.Id && b.AccountId == accountId && b.IsActive,
                cancellationToken);

            if (existingBindings.Any())
            {
                return (false, null, "该微信账号已绑定其他本地账号");
            }

            var newBinding = new WechatUserBinding
            {
                Id = new SequentialGuid().Create(),
                WechatUserId = wechatUser.Id,
                LocalUserId = localUserId,
                AccountId = accountId,
                OpenId = openId,
                BindingTime = DateTime.UtcNow,
                IsActive = true
            };

            await bindingRepository.InsertAsync(newBinding, cancellationToken);

            return (true, newBinding.LocalUserId.ToString(), "绑定成功");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Token, string? Message)> UnbindAsync(
        Guid accountId,
        string openId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wechatUsers = await wechatUserRepository.FindListAsync(
                u => u.AccountId == accountId && u.OpenId == openId,
                cancellationToken);

            var wechatUser = wechatUsers.FirstOrDefault();
            if (wechatUser == null)
            {
                return (false, null, "微信用户不存在");
            }

            var bindings = await bindingRepository.FindListAsync(
                b => b.WechatUserId == wechatUser.Id && b.AccountId == accountId && b.IsActive,
                cancellationToken);

            var binding = bindings.FirstOrDefault();
            if (binding == null)
            {
                return (false, null, "未绑定本地账号");
            }

            binding.IsActive = false;
            await bindingRepository.UpdateAsync(binding, cancellationToken);

            return (true, null, "解绑成功");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<Guid?> GetLocalUserIdAsync(Guid accountId, string openId, CancellationToken cancellationToken = default)
    {
        var wechatUsers = await wechatUserRepository.FindListAsync(
            u => u.AccountId == accountId && u.OpenId == openId,
            cancellationToken);

        var wechatUser = wechatUsers.FirstOrDefault();
        if (wechatUser == null) return null;

        var bindings = await bindingRepository.FindListAsync(
            b => b.WechatUserId == wechatUser.Id && b.AccountId == accountId && b.IsActive,
            cancellationToken);

        return bindings.FirstOrDefault()?.LocalUserId;
    }
}