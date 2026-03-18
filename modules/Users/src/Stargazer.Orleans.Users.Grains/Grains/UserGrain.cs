using Orleans.Concurrency;
using Stargazer.Common;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.UserRoles;
using Stargazer.Orleans.Users.Domain.Users;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Grains.Grains;

[StatelessWorker]
public class UserGrain(
    IRepository<UserData, Guid> userRepository,
    IRepository<UserRoleData, Guid> userRoleRepository,
    IRepository<RoleData, Guid> roleRepository) : Grain, IUserGrain
{
    public async Task ChangePasswordAsync(Guid id, ChangePasswordInputDto input, Guid modifierId,
        CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.GetAsync(id, cancellationToken);
        userData.Password = Cryptography.PasswordStorage.CreateHash(input.NewPassword, out string secretKey);
        userData.SecretKey = secretKey;
        userData.LastModifierId = modifierId;
        userData.LastModifyTime = DateTime.UtcNow;
        await userRepository.UpdateAsync(userData, cancellationToken);
    }

    public async Task<bool> VerifyPasswordAsync(VerifyPasswordInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(x => x.Account.Equals(input.Name), cancellationToken);
        if (userData is null || !userData.IsActive)
        {
            return false;
        }
        return Cryptography.PasswordStorage.VerifyPassword(input.Password, userData.Password, userData.SecretKey);
    }

    public async Task<UserDataDto> RegisterAsync(RegisterAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = new UserData()
        {
            Id = new SequentialGuid().Create(),
            Account = input.Account,
            Password = Cryptography.PasswordStorage.CreateHash(input.Password, out string secretKey),
            SecretKey = secretKey,
            IsActive = true,
            CreationTime = DateTime.UtcNow
        };
        
        var defaultRole = await roleRepository.FindAsync(x => x.IsDefault && x.IsActive, cancellationToken);
        if (defaultRole is not null)
        {
            var userRole = new UserRoleData
            {
                Id = new SequentialGuid().Create(),
                UserId = userData.Id,
                RoleId = defaultRole.Id,
                IsActive = true,
                CreationTime = DateTime.UtcNow
            };
            userData.UserRoles.Add(userRole);
        }
        
        var result = await userRepository.InsertAsync(userData, cancellationToken);
        return result.MapToUserDto();
    }

    public async Task<UserDataDto?> GetUserDataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(id, cancellationToken);
        return userData?.MapToUserDto();
    }

    public async Task<UserDataDto?> GetUserDataAsync(string name, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(x => x.Account == name || x.Email == name || x.PhoneNumber == name, cancellationToken);
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
            IsActive = input.IsActive,
            CreationTime = DateTime.UtcNow
        };
        await userRepository.InsertAsync(userData, cancellationToken);
    }

    public async Task UpdateUserAsync(Guid id, CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.GetAsync(id, cancellationToken);
        userData.Account = input.Account;
        userData.Password = Cryptography.PasswordStorage.CreateHash(input.Password, out string secretKey);
        userData.SecretKey = secretKey;
        userData.Name = input.Name;
        userData.Email = input.Email;
        userData.PhoneNumber = input.PhoneNumber;
        userData.Avatar = input.Avatar;
        userData.IsActive = input.IsActive;
        userData.LastModifyTime = DateTime.UtcNow;
        await userRepository.UpdateAsync(userData, cancellationToken);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(id, cancellationToken);
        if (userData is not null)
        {
            var userRoles = await userRoleRepository.FindListAsync(x => x.UserId == id, cancellationToken);
            if (userRoles.Any())
            {
                await userRoleRepository.DeleteManyAsync(userRoles.Select(x => x.Id), cancellationToken);
            }
            await userRepository.DeleteAsync(id, cancellationToken);
        }
    }

    public async Task<bool> AccountExistedAsync(string account, CancellationToken cancellationToken = default)
    {
        return await userRepository.AnyAsync(x => x.Account == account, cancellationToken);
    }

    public async Task<bool> NameExistedAsync(string name, CancellationToken cancellationToken = default)
    {
        return await userRepository.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<bool> EmailExistedAsync(string email, CancellationToken cancellationToken = default)
    {
        return await userRepository.AnyAsync(x => x.Email == email, cancellationToken); 
    }

    public async Task<bool> PhoneNumberExistedAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await userRepository.AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<PageResult<UserDataDto>> GetUsersAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = userRepository.GetQueryable();
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(x => x.Account.Contains(keyword) || x.Name.Contains(keyword) || x.Email.Contains(keyword) || x.PhoneNumber.Contains(keyword));
        }
        var total = query.Count();
        var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(x => x.MapToUserDto()).ToList();
        return new PageResult<UserDataDto>
        {
            Total = total,
            Items = items
        };
    }

    public async Task<UserDataDto?> GetUserByAccountAsync(string account, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(x => x.Account == account, cancellationToken);
        return userData?.MapToUserDto();
    }

    public async Task<bool> DisableUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(id, cancellationToken);
        if (userData is null) return false;
        
        userData.IsActive = false;
        userData.LastModifyTime = DateTime.UtcNow;
        await userRepository.UpdateAsync(userData, cancellationToken);
        return true;
    }

    public async Task<bool> EnableUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(id, cancellationToken);
        if (userData is null) return false;
        
        userData.IsActive = true;
        userData.LastModifyTime = DateTime.UtcNow;
        await userRepository.UpdateAsync(userData, cancellationToken);
        return true;
    }

    public async Task<UserDataDto?> GetUserProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(id, cancellationToken);
        return userData?.MapToUserDto();
    }

    public async Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var userData = await userRepository.FindAsync(userId, cancellationToken);
        if (userData is null) return false;
        
        var existingUserRoles = await userRoleRepository.FindListAsync(x => x.UserId == userId, cancellationToken);
        await userRoleRepository.DeleteManyAsync(existingUserRoles.Select(x => x.Id), cancellationToken);
        
        foreach (var roleId in roleIds)
        {
            var userRole = new UserRoleData
            {
                Id = new SequentialGuid().Create(),
                UserId = userId,
                RoleId = roleId,
                IsActive = true,
                CreationTime = DateTime.UtcNow
            };
            await userRoleRepository.InsertAsync(userRole, cancellationToken);
        }
        
        return true;
    }

    public async Task<List<RoleDataDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await userRoleRepository.FindListAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
        var roleIds = userRoles.Select(x => x.RoleId).ToList();
        
        var roles = await roleRepository.FindListAsync(x => roleIds.Contains(x.Id) && x.IsActive, cancellationToken);
        return roles.Select(x => x.MapToRoleDto()).ToList();
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var userRoles = await userRoleRepository.FindListAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
        var roleIds = userRoles.Select(x => x.RoleId).ToList();
        
        var roles = await roleRepository.FindListAsync(x => roleIds.Contains(x.Id) && x.IsActive, cancellationToken);
        
        return roles.Any(role => role.Permissions.Any(p => p.Code == permissionCode && p.IsActive));
    }

    public async Task<List<PermissionDataDto>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await userRoleRepository.FindListAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
        var roleIds = userRoles.Select(x => x.RoleId).ToList();
        
        var roles = await roleRepository.FindListAsync(x => roleIds.Contains(x.Id) && x.IsActive, cancellationToken);
        
        var permissions = roles.SelectMany(x => x.Permissions).DistinctBy(p => p.Id).Where(p => p.IsActive).ToList();
        return permissions.Select(x => x.MapToPermissionDto()).ToList();
    }
}
