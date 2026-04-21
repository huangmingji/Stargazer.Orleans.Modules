using Orleans;
using Orleans.Concurrency;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.WechatManagement.Domain.Users;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Users;

[StatelessWorker]
public class WechatUserGroupGrain(
    IRepository<WechatUserGroup, Guid> repository,
    IRepository<WechatUser, Guid> userRepository)
    : Grain, IWechatUserGroupGrain
{
    public async Task<WechatUserGroupDto?> GetGroupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await repository.FindAsync(id, cancellationToken);
        return group == null ? null : MapToDto(group);
    }

    public async Task<WechatUserGroupDto> CreateGroupAsync(CreateWechatUserGroupInputDto input, CancellationToken cancellationToken = default)
    {
        var group = new WechatUserGroup
        {
            Id = new SequentialGuid().Create(),
            AccountId = input.AccountId,
            Name = input.Name,
            UserCount = 0,
            CreationTime = DateTime.UtcNow,
            LastModifyTime = DateTime.UtcNow
        };

        await repository.InsertAsync(group, cancellationToken);
        return MapToDto(group);
    }

    public async Task<WechatUserGroupDto?> UpdateGroupAsync(Guid id, UpdateWechatUserGroupInputDto input, CancellationToken cancellationToken = default)
    {
        var group = await repository.FindAsync(id, cancellationToken);
        if (group == null) return null;

        if (input.Name != null) group.Name = input.Name;
        group.LastModifyTime = DateTime.UtcNow;

        await repository.UpdateAsync(group, cancellationToken);
        return MapToDto(group);
    }

    public async Task<bool> DeleteGroupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await repository.FindAsync(id, cancellationToken);
        if (group == null) return false;

        await repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<int> GetUserCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await repository.FindAsync(id, cancellationToken);
        if (group == null) return 0;

        return await userRepository.CountAsync(
            u => u.GroupId == group.Id && u.SubscribeStatus == 1,
            cancellationToken);
    }

    public async Task SyncFromWechatAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await repository.FindAsync(id, cancellationToken);
        if (group == null) return;

        group.UserCount = await GetUserCountAsync(id, cancellationToken);
        await repository.UpdateAsync(group, cancellationToken);
    }

    private static WechatUserGroupDto MapToDto(WechatUserGroup group)
    {
        return new WechatUserGroupDto
        {
            Id = group.Id,
            AccountId = group.AccountId,
            Name = group.Name,
            WechatGroupId = group.WechatGroupId,
            UserCount = group.UserCount,
            CreationTime = group.CreationTime
        };
    }
}