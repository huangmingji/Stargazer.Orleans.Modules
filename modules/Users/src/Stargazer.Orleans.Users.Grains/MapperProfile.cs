using Riok.Mapperly.Abstractions;
using Stargazer.Orleans.Users.Domain.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Grains;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class MapperProfile
{
    public static partial UserDataDto MapToUserDto(this UserData user);

    public static partial List<UserDataDto> MapToUserDtos(this List<UserData> users);
}

