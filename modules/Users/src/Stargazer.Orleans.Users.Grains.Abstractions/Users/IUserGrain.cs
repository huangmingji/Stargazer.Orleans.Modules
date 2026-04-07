using Orleans;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
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
    
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> AccountExistedAsync(string account, CancellationToken cancellationToken = default);
    
    Task<bool> NameExistedAsync(string name, CancellationToken cancellationToken = default);
    
    Task<bool> EmailExistedAsync(string email, CancellationToken cancellationToken = default);
    
    Task<bool> PhoneNumberExistedAsync(string phoneNumber, CancellationToken cancellationToken = default);
    
    Task<UserDataDto?> GetUserByAccountAsync(string account, CancellationToken cancellationToken = default);
    
    Task<bool> DisableUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> EnableUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateUserStatusAsync(Guid id, UpdateUserStatusInputDto input, CancellationToken cancellationToken = default);
    
    Task<UserDataDto?> GetUserProfileAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<UserDataDto?> UpdateProfileAsync(Guid id, UpdateProfileInputDto input, CancellationToken cancellationToken = default);
    
    Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default);
    
    Task<List<RoleDataDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
    
    Task<List<PermissionDataDto>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
