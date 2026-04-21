using System.Security.Cryptography;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.WechatManagement.Domain.Accounts;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Accounts;

[StatelessWorker]
public class WechatAccountGrain(IRepository<WechatAccount, Guid> repository) : Grain, IWechatAccountGrain
{
    public async Task<WechatAccountDto?> GetAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await repository.FindAsync(id, cancellationToken);
        return account == null ? null : MapToDto(account);
    }

    public async Task<WechatAccountDto> CreateAccountAsync(CreateWechatAccountInputDto input, CancellationToken cancellationToken = default)
    {
        if (input.IsDefault)
        {
            var existingDefaults = await repository.FindListAsync(a => a.IsDefault, cancellationToken);
            foreach (var existingAccount in existingDefaults)
            {
                existingAccount.IsDefault = false;
                await repository.UpdateAsync(existingAccount, cancellationToken);
            }
        }

        var newAccount = new WechatAccount
        {
            Id = new SequentialGuid().Create(),
            Name = input.Name,
            AppId = input.AppId,
            AppSecret = input.AppSecret,
            Token = input.Token,
            EncodingAESKey = input.EncodingAESKey,
            IsDefault = input.IsDefault,
            IsActive = true,
            CreationTime = DateTime.UtcNow,
            LastModifyTime = DateTime.UtcNow
        };

        await repository.InsertAsync(newAccount, cancellationToken);
        return MapToDto(newAccount);
    }

    public async Task<WechatAccountDto?> UpdateAccountAsync(Guid id, UpdateWechatAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var account = await repository.FindAsync(id, cancellationToken);
        if (account == null) return null;

        if (input.Name != null) account.Name = input.Name;
        if (input.AppSecret != null) account.AppSecret = input.AppSecret;
        if (input.Token != null) account.Token = input.Token;
        if (input.EncodingAESKey != null) account.EncodingAESKey = input.EncodingAESKey;
        if (input.IsDefault.HasValue)
        {
            if (input.IsDefault.Value)
            {
                var existingDefaults = await repository.FindListAsync(a => a.IsDefault && a.Id != account.Id, cancellationToken);
                foreach (var acc in existingDefaults)
                {
                    acc.IsDefault = false;
                    await repository.UpdateAsync(acc, cancellationToken);
                }
            }
            account.IsDefault = input.IsDefault.Value;
        }
        if (input.IsActive.HasValue) account.IsActive = input.IsActive.Value;
        account.LastModifyTime = DateTime.UtcNow;

        await repository.UpdateAsync(account, cancellationToken);
        return MapToDto(account);
    }

    public async Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await repository.FindAsync(id, cancellationToken);
        if (account == null) return false;

        await repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<List<WechatAccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await repository.FindListAsync(a => a.IsActive, cancellationToken);
        return accounts
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Name)
            .Select(a => MapToDto(a))
            .ToList();
    }

    public async Task<string?> GetAccessTokenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await repository.FindAsync(id, cancellationToken);
        if (account == null) return null;

        if (!account.AccessTokenExpiry.HasValue || account.AccessTokenExpiry.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return account.AccessToken;
        }

        return null;
    }

    public async Task SetAccessTokenAsync(Guid id, string token, DateTime expiry, CancellationToken cancellationToken = default)
    {
        var account = await repository.FindAsync(id, cancellationToken);
        if (account == null) return;

        account.AccessToken = token;
        account.AccessTokenExpiry = expiry;
        account.LastModifyTime = DateTime.UtcNow;

        await repository.UpdateAsync(account, cancellationToken);
    }

    public async Task<bool> ValidateCallbackAsync(Guid id, string signature, string timestamp, string nonce,
        string echostr, CancellationToken cancellationToken = default)
    {
        var account = await repository.FindAsync(id, cancellationToken);
        if (account == null) return false;

        var arr = new[] {account.Token, timestamp, nonce}.OrderBy(x => x).ToArray();
        var arrString = string.Join("", arr);
        var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(arrString));
        var expectedSignature = BitConverter.ToString(sha1).Replace("-", "").ToLower();

        return signature == expectedSignature;
    }

    private static WechatAccountDto MapToDto(WechatAccount account, bool maskAppSecret = true)
    {
        return new WechatAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            AppId = account.AppId,
            AppSecret = maskAppSecret ? "******" : account.AppSecret,
            Token = account.Token,
            EncodingAESKey = account.EncodingAESKey,
            IsDefault = account.IsDefault,
            IsActive = account.IsActive,
            AccessTokenExpiry = account.AccessTokenExpiry,
            CreationTime = account.CreationTime,
            LastModifyTime = account.LastModifyTime
        };
    }
}