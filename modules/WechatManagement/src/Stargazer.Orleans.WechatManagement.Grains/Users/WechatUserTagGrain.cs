using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Concurrency;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.WechatManagement.Domain.Users;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Users;

[StatelessWorker]
public class WechatUserTagGrain(
    IRepository<WechatUserTag, Guid> wechatUsertagRepository,
    IRepository<WechatUser, Guid> wechatUserRepository)
    : Grain, IWechatUserTagGrain
{
    public async Task<WechatUserTagDto?> GetTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        return tag == null ? null : MapToDto(tag);
    }

    public async Task<WechatUserTagDto> CreateTagAsync(CreateWechatUserTagInputDto input, CancellationToken cancellationToken = default)
    {
        var tag = new WechatUserTag
        {
            Id = new SequentialGuid().Create(),
            AccountId = input.AccountId,
            Name = input.Name,
            UserCount = 0,
            CreationTime = DateTime.UtcNow,
            LastModifyTime = DateTime.UtcNow
        };

        await wechatUsertagRepository.InsertAsync(tag, cancellationToken);
        return MapToDto(tag);
    }

    public async Task<WechatUserTagDto?> UpdateTagAsync(Guid id, UpdateWechatUserTagInputDto input, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        if (tag == null) return null;

        if (input.Name != null) tag.Name = input.Name;
        tag.LastModifyTime = DateTime.UtcNow;

        await wechatUsertagRepository.UpdateAsync(tag, cancellationToken);
        return MapToDto(tag);
    }

    public async Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        if (tag == null) return false;

        await wechatUsertagRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<int> GetUserCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        if (tag == null) return 0;

        return tag.UserCount;
    }

    public async Task AddUserToTagAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        if (tag == null) return;

        var user = await wechatUserRepository.GetQueryable().Include(u => u.Tags)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user != null && user.Tags.All(t => t.Id != tag.Id))
        {
            user.Tags.Add(tag);
            tag.UserCount++;
            await wechatUserRepository.UpdateAsync(user, cancellationToken);
        }
    }

    public async Task RemoveUserFromTagAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        if (tag == null) return;

        var user = await wechatUserRepository.GetQueryable().Include(u => u.Tags)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user != null)
        {
            var tagToRemove = user.Tags.FirstOrDefault(t => t.Id == tag.Id);
            if (tagToRemove != null)
            {
                user.Tags.Remove(tagToRemove);
                tag.UserCount = Math.Max(0, tag.UserCount - 1);
                await wechatUserRepository.UpdateAsync(user, cancellationToken);
            }
        }
    }

    public async Task SyncFromWechatAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await wechatUsertagRepository.FindAsync(id, cancellationToken);
        if (tag == null) return;

        var userCount = await wechatUserRepository.CountAsync(
            u => u.Tags.Any(t => t.Id == id),
            cancellationToken);

        tag.UserCount = userCount;
        await wechatUsertagRepository.UpdateAsync(tag, cancellationToken);
    }

    private static WechatUserTagDto MapToDto(WechatUserTag tag)
    {
        return new WechatUserTagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            WechatTagId = tag.WechatTagId,
            UserCount = tag.UserCount
        };
    }
}