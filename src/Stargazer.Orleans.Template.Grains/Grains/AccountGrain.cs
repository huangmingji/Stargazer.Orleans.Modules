using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Stargazer.Orleans.Template.Domain.Users;
using Stargazer.Orleans.Template.Grains.Abstractions.Users;
using Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;
using Stargazer.Orleans.Utility.ExceptionExtensions;
using Stargazer.Orleans.Utility.Extend;
using Stargazer.Orleans.Utility.SequentialGuid;

namespace Stargazer.Orleans.Template.Grains.Grains;

public class AccountGrain([PersistentState("state", "Default")] IPersistentState<Account> state,
    ILogger<AccountGrain> logger)
    : Grain, IAccountGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (state.State.Id.IsEmpty())
        {
            await state.ReadStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }
    
    public async Task<AccountDto> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        return new AccountDto()
        {
            Id = state.State.Id,
            AccountName = state.State.AccountName,
            CreateTime = state.State.CreateTime,
            CreatorId = state.State.CreatorId,
            LastModifyTime = state.State.LastModifyTime,
            LastModifierId = state.State.LastModifierId
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordInputDto input, Guid modifierId, CancellationToken cancellationToken = default)
    {
        state.State.Password = input.NewPassword;
        state.State.LastModifierId = modifierId;
        await state.WriteStateAsync(cancellationToken);
    }

    public async Task VerifyPasswordAsync(VerifyPasswordInputDto input, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(state.State.SerializeObject());
        if (state.State.Id.IsEmpty())
        {
            throw new VerifyPasswordErrorException("Account not found");
        }

        if (state.State.Password != input.Password)
        {
            throw new VerifyPasswordErrorException("Password error");
        }
    }

    public async Task<AccountDto> RegisterAsync(RegisterAccountInputDto inputDto, CancellationToken cancellationToken = default)
    {
        if (state.State.Id == Guid.Empty)
        {
            await state.ReadStateAsync(cancellationToken);
        }

        state.State.Id = new SequentialGuid().Create();
        state.State.AccountName = inputDto.AccountName;
        state.State.Password = inputDto.Password;
        await state.WriteStateAsync(cancellationToken);
        return new AccountDto()
        {
            Id = state.State.Id,
            AccountName = state.State.AccountName,
            CreateTime = state.State.CreateTime,
            CreatorId = state.State.CreatorId,
            LastModifyTime = state.State.LastModifyTime,
            LastModifierId = state.State.LastModifierId
        };
    }
}