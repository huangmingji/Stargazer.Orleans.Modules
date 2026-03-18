using Riok.Mapperly.Abstractions;
using Stargazer.Orleans.Users.Domain.Permissions;
using Stargazer.Orleans.Users.Domain.Roles;
using Stargazer.Orleans.Users.Domain.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Grains;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class MapperProfile
{
    public static partial UserDataDto MapToUserDto(this UserData user);

    public static partial List<UserDataDto> MapToUserDtos(this List<UserData> users);
    
    public static partial RoleDataDto MapToRoleDto(this RoleData role);
    
    public static partial List<RoleDataDto> MapToRoleDtos(this List<RoleData> roles);
    
    public static partial PermissionDataDto MapToPermissionDto(this PermissionData permission);
    
    public static partial List<PermissionDataDto> MapToPermissionDtos(this List<PermissionData> permissions);
}

