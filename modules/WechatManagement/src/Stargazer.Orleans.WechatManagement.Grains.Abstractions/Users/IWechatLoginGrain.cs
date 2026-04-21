namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IWechatLoginGrain : IGrainWithIntegerKey
{
    Task<string> GenerateQrCodeAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Token, string? Message)> ProcessScanResultAsync(
        Guid accountId,
        string openId,
        string sceneId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Token, string? Message)> BindLocalUserAsync(
        Guid accountId,
        string openId,
        Guid localUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Token, string? Message)> UnbindAsync(
        Guid accountId,
        string openId,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetLocalUserIdAsync(Guid accountId, string openId, CancellationToken cancellationToken = default);
}