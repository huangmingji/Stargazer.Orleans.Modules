using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Stargazer.Orleans.Template.Domain.Users;
using Stargazer.Orleans.Template.Grains.Abstractions.Users;
using Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;
using Stargazer.Orleans.Utility.Extend;

namespace Stargazer.Orleans.Template.Grains.Grains;

public class UserGrain([PersistentState("state", "Default")] IPersistentState<UserData> state,
    ILogger<UserGrain> logger)
    : Grain, IUserGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (state.State.Id.IsEmpty())
        {
            await state.ReadStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<UserDataDto?> GetUserDataAsync(CancellationToken cancellationToken = default)
    {
        if (state.State.Id == Guid.Empty)
        {
            return null;
        }

        return await Task.FromResult(new UserDataDto()
        {
            Id = state.State.Id,
            AccountId = state.State.AccountId,
            UserName = state.State.UserName,
            Email = state.State.Email,
            PhoneNumber = state.State.PhoneNumber,
            Avatar = state.State.Avatar,
            CreateTime = state.State.CreationTime,
            CreatorId = state.State.CreatorId,
            LastModifyTime = state.State.LastModifyTime,
            LastModifierId = state.State.LastModifierId
        });
    }

    public async Task<UserDataDto> CreateUserAsync(CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        state.State.Id = this.GetPrimaryKey();
        state.State.UserName = input.UserName;
        state.State.Email = input.Email;
        state.State.PhoneNumber = input.PhoneNumber;
        state.State.Avatar = input.Avatar;
        await state.WriteStateAsync(cancellationToken);
        return new UserDataDto()
        {
            Id = state.State.Id,
            AccountId = state.State.AccountId,
            UserName = state.State.UserName,
            Email = state.State.Email,
            PhoneNumber = state.State.PhoneNumber,
            Avatar = state.State.Avatar,
            CreateTime = state.State.CreationTime,
            CreatorId = state.State.CreatorId,
            LastModifyTime = state.State.LastModifyTime,
            LastModifierId = state.State.LastModifierId
        };
    }

    public async Task<UserDataDto?> UpdateUserAsync(CreateOrUpdateUserInputDto input,
        CancellationToken cancellationToken = default)
    {
        if (state.State.Id == Guid.Empty)
        {
            return null;
        }

        state.State.UserName = input.UserName;
        state.State.Email = input.Email;
        state.State.PhoneNumber = input.PhoneNumber;
        state.State.Avatar = input.Avatar;
        await state.WriteStateAsync(cancellationToken);
        return await Task.FromResult(new UserDataDto()
        {
            Id = state.State.Id,
            AccountId = state.State.AccountId,
            UserName = state.State.UserName,
            Email = state.State.Email,
            PhoneNumber = state.State.PhoneNumber,
            Avatar = state.State.Avatar,
            CreateTime = state.State.CreationTime,
            CreatorId = state.State.CreatorId,
            LastModifyTime = state.State.LastModifyTime,
            LastModifierId = state.State.LastModifierId
        });
    }

    public async Task DeleteUserAsync(CancellationToken cancellationToken = default)
    {
        if (state.State.Id.IsEmpty())
        {
            return;
        }
        await state.ClearStateAsync(cancellationToken);
        state.State.Id = Guid.Empty;
    }
}