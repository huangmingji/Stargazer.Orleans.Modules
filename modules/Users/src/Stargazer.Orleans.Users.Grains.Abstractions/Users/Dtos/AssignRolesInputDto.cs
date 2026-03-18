using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class AssignRolesInputDto
{
    [Id(0)]
    public Guid UserId { get; set; }

    [Id(1)]
    public List<Guid> RoleIds { get; set; } = new();
}
