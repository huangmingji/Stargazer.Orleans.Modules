using Orleans.Concurrency;
using Stargazer.Common;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.Users.Domain.Users;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Grains.Grains;

[StatelessWorker]
public class UserGrain(IRepository<UserData, Guid> repository) : Grain, IUserGrain
{
    public async Task ChangePasswordAsync(Guid id, ChangePasswordInputDto input, Guid modifierId,
        CancellationToken cancellationToken = default)
    {
        var userData = await repository.GetAsync(id, cancellationToken);
        userData.Password = Cryptography.PasswordStorage.CreateHash(input.NewPassword, out string secretKey);
        userData.SecretKey = secretKey;
        userData.LastModifierId = modifierId;
        userData.LastModifyTime = DateTime.UtcNow;
        await repository.UpdateAsync(userData, cancellationToken);
    }

    public async Task<bool> VerifyPasswordAsync(VerifyPasswordInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = await repository.FindAsync(x => x.Account.Equals(input.Name), cancellationToken);
        return userData is not null && Cryptography.PasswordStorage.VerifyPassword(input.Password, userData.Password, userData.SecretKey);
    }

    public async Task<UserDataDto> RegisterAsync(RegisterAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = new UserData()
        {
            Id = new SequentialGuid().Create(),
            Account = input.Account,
            Password = Cryptography.PasswordStorage.CreateHash(input.Password, out string secretKey),
            SecretKey = secretKey,
            CreationTime = DateTime.UtcNow
        };
        var result = await repository.InsertAsync(userData, cancellationToken);
        return result.MapToUserDto();
    }

    public async Task<UserDataDto?> GetUserDataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await repository.FindAsync(id, cancellationToken);
        return userData?.MapToUserDto();
    }

    public async Task<UserDataDto?> GetUserDataAsync(string name, CancellationToken cancellationToken = default)
    {
        var userData = await repository.FindAsync(x => x.Account == name || x.Email == name || x.PhoneNumber == name, cancellationToken);
        return userData?.MapToUserDto();
    }

    public async Task CreateUserAsync(CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = new UserData()
        {
            Id = new SequentialGuid().Create(),
            Account = input.Account,
            Password = Cryptography.PasswordStorage.CreateHash(input.Password, out string secretKey),
            SecretKey = secretKey,
            Name = input.Name,
            Email = input.Email,
            PhoneNumber = input.PhoneNumber,
            Avatar = input.Avatar,
            CreationTime = DateTime.UtcNow
        };
        await repository.InsertAsync(userData, cancellationToken);
    }

    public async Task UpdateUserAsync(Guid id, CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = await repository.GetAsync(id, cancellationToken);
        userData.Account = input.Account;
        userData.Password = Cryptography.PasswordStorage.CreateHash(input.Password, out string secretKey);
        userData.SecretKey = secretKey;
        userData.Name = input.Name;
        userData.Email = input.Email;
        userData.PhoneNumber = input.PhoneNumber;
        userData.Avatar = input.Avatar;
        userData.LastModifyTime = DateTime.UtcNow;
        await repository.UpdateAsync(userData, cancellationToken);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await repository.FindAsync(id, cancellationToken);
        if (userData is not null)
        {
            await repository.DeleteAsync(id, cancellationToken);
        }
    }

    public async Task<bool> AccountExistedAsync(string account, CancellationToken cancellationToken = default)
    {
        return await repository.AnyAsync(x => x.Account == account, cancellationToken);
    }

    public async Task<bool> NameExistedAsync(string name, CancellationToken cancellationToken = default)
    {
        return await repository.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<bool> EmailExistedAsync(string email, CancellationToken cancellationToken = default)
    {
        return await repository.AnyAsync(x => x.Email == email, cancellationToken); 
    }

    public async Task<bool> PhoneNumberExistedAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await repository.AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<PageResult<UserDataDto>> GetUsersAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = repository.GetQueryable();
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(x => x.Account.Contains(keyword) || x.Name.Contains(keyword) || x.Email.Contains(keyword) || x.PhoneNumber.Contains(keyword));
        }
        var total = query.Count();
        var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(x => x.MapToUserDto()).ToList();
        return await Task.FromResult(new PageResult<UserDataDto>()
        {
            Total = total,
            Items = items
        });
    }
}
