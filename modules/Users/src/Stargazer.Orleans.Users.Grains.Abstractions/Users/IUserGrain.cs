using Orleans;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users;

public interface IUserGrain : IGrainWithIntegerKey
{
    Task ChangePasswordAsync(Guid id, ChangePasswordInputDto input, Guid modifierId, CancellationToken cancellationToken = default);

    Task<bool> VerifyPasswordAsync(VerifyPasswordInputDto input, CancellationToken cancellationToken = default);

    Task<UserDataDto> RegisterAsync(RegisterAccountInputDto input, CancellationToken cancellationToken = default);
    
    Task<UserDataDto?> GetUserDataAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<UserDataDto?> GetUserDataAsync(string name, CancellationToken cancellationToken = default);

    Task<PageResult<UserDataDto>> GetUsersAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    
    Task CreateUserAsync(CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default);
    
    Task UpdateUserAsync(Guid id, CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default);
    
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> AccountExistedAsync(string account, CancellationToken cancellationToken = default);
    
    Task<bool> NameExistedAsync(string name, CancellationToken cancellationToken = default);
    
    Task<bool> EmailExistedAsync(string email, CancellationToken cancellationToken = default);
    
    Task<bool> PhoneNumberExistedAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
