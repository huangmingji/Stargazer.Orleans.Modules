using System.Linq.Expressions;
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
public class WechatUserGrain(
    IRepository<WechatUser, Guid> wechatUserRepository,
    IRepository<WechatUserTag, Guid> wechatUserTagRepository)
    : Grain, IWechatUserGrain
{

    public async Task<WechatUserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.FindAsync(id, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    public async Task<WechatUserDto?> GetUserByOpenIdAsync(Guid accountId, string openId, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.GetAsync(u => u.OpenId == openId && u.AccountId == accountId, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    public async Task<WechatUserDto> CreateUserAsync(CreateWechatUserInputDto input, CancellationToken cancellationToken = default)
    {
        var user = new WechatUser
        {
            Id = new SequentialGuid().Create(),
            OpenId = input.OpenId,
            UnionId = input.UnionId,
            AccountId = input.AccountId,
            Nickname = input.Nickname,
            Sex = input.Sex,
            Province = input.Province,
            City = input.City,
            Country = input.Country,
            HeadimgUrl = input.HeadimgUrl,
            SubscribeTime = input.SubscribeTime,
            SubscribeStatus = 1,
            GroupId = input.GroupId,
            CreationTime = DateTime.UtcNow,
            LastModifyTime = DateTime.UtcNow
        };

        await wechatUserRepository.InsertAsync(user, cancellationToken);
        return MapToDto(user);
    }

    public async Task<WechatUserDto?> UpdateUserAsync(Guid id, UpdateWechatUserInputDto input, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.FindAsync(id, cancellationToken);
        if (user == null) return null;

        if (input.Remark != null) user.Remark = input.Remark;
        if (input.GroupId.HasValue) user.GroupId = input.GroupId;
        user.LastModifyTime = DateTime.UtcNow;

        await wechatUserRepository.UpdateAsync(user, cancellationToken);
        return MapToDto(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.FindAsync(id, cancellationToken);
        if (user == null) return false;

        await wechatUserRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<bool> SaveUserInfoAsync(
        Guid accountId,
        string openId,
        string? unionId,
        string nickname,
        int sex,
        string? province,
        string? city,
        string? country,
        string? headimgUrl,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await wechatUserRepository.FindListAsync(
            u => u.AccountId == accountId && u.OpenId == openId,
            cancellationToken);

        var user = existingUser.FirstOrDefault();
        var isNew = user == null;

        if (isNew)
        {
            user = new WechatUser
            {
                Id = new SequentialGuid().Create(),
                AccountId = accountId,
                OpenId = openId,
                UnionId = unionId,
                Nickname = nickname,
                Sex = sex,
                Province = province,
                City = city,
                Country = country,
                HeadimgUrl = headimgUrl,
                SubscribeStatus = 1,
                SubscribeTime = DateTime.UtcNow,
                CreationTime = DateTime.UtcNow,
                LastModifyTime = DateTime.UtcNow
            };
            await wechatUserRepository.InsertAsync(user, cancellationToken);
        }
        else
        {
            user.Nickname = nickname;
            user.Sex = sex;
            user.Province = province;
            user.City = city;
            user.Country = country;
            user.HeadimgUrl = headimgUrl;
            user.LastModifyTime = DateTime.UtcNow;
            await wechatUserRepository.UpdateAsync(user, cancellationToken);
        }

        return isNew;
    }

    public async Task SubscribeAsync(Guid id, string openId, int subscribeStatus, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.FindAsync(id, cancellationToken);
        if (user == null) return;

        user.SubscribeStatus = subscribeStatus;
        if (subscribeStatus == 1)
        {
            user.SubscribeTime = DateTime.UtcNow;
            user.UnSubscribeTime = null;
        }
        else
        {
            user.UnSubscribeTime = DateTime.UtcNow;
        }
        user.LastModifyTime = DateTime.UtcNow;

        await wechatUserRepository.UpdateAsync(user, cancellationToken);
    }

    public Task UnSubscribeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SubscribeAsync(id, "", 0, cancellationToken);
    }

    public async Task AddTagAsync(Guid id, Guid tagId, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.FindAsync(id, cancellationToken);
        if (user == null) return;
        
        var tag = await wechatUserTagRepository.GetQueryable().Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == tagId, cancellationToken);

        if (tag != null && tag.Users.All(u => u.Id != user.Id))
        {
            tag.Users.Add(user);
            tag.UserCount++;
            await wechatUserTagRepository.UpdateAsync(tag, cancellationToken);
        }
    }

    public async Task RemoveTagAsync(Guid id, Guid tagId, CancellationToken cancellationToken = default)
    {
        var user = await wechatUserRepository.FindAsync(id, cancellationToken);
        if (user == null) return;

        var tag = await wechatUserTagRepository.GetQueryable().Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == tagId, cancellationToken);

        if (tag != null)
        {
            var tagUser = tag.Users.FirstOrDefault(u => u.Id == user.Id);
            if (tagUser != null)
            {
                tag.Users.Remove(tagUser);
                tag.UserCount = Math.Max(0, tag.UserCount - 1);
                await wechatUserTagRepository.UpdateAsync(tag, cancellationToken);
            }
        }
    }

    public async Task<(List<WechatUserDto> Items, int Total)> GetFansAsync(
        Guid accountId,
        int page,
        int pageSize,
        int? subscribeStatus,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<WechatUser, bool>> predicate = u => true;

        if (accountId != Guid.Empty && subscribeStatus.HasValue)
        {
            predicate = u => u.AccountId == accountId && u.SubscribeStatus == subscribeStatus.Value;
        }
        else if (accountId != Guid.Empty)
        {
            predicate = u => u.AccountId == accountId;
        }
        else if (subscribeStatus.HasValue)
        {
            predicate = u => u.SubscribeStatus == subscribeStatus.Value;
        }

        var (users, total) = await wechatUserRepository.FindListAsync(
            predicate,
            page,
            pageSize,
            u => u.SubscribeTime,
            true,
            q => q.Include(u => u.Group).Include(u => u.Tags),
            cancellationToken);

        var dtos = users.Select(MapToDto).ToList();

        return (dtos, total);
    }

    private static WechatUserDto MapToDto(WechatUser user)
    {
        return new WechatUserDto
        {
            Id = user.Id,
            OpenId = user.OpenId,
            UnionId = user.UnionId,
            AccountId = user.AccountId,
            Nickname = user.Nickname,
            Sex = user.Sex,
            Province = user.Province,
            City = user.City,
            Country = user.Country,
            HeadimgUrl = user.HeadimgUrl,
            SubscribeTime = user.SubscribeTime,
            UnSubscribeTime = user.UnSubscribeTime,
            Remark = user.Remark,
            SubscribeStatus = user.SubscribeStatus,
            GroupId = user.GroupId,
            GroupName = user.Group?.Name,
            Tags = user.Tags?.Select(t => new WechatUserTagDto
            {
                Id = t.Id,
                Name = t.Name,
                WechatTagId = t.WechatTagId,
                UserCount = t.UserCount
            }).ToList() ?? new(),
            CreationTime = user.CreationTime
        };
    }
}