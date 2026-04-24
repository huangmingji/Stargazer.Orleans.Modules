using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class AssignRolesInputDto
{
    [Id(0)]
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [Id(1)]
    [JsonPropertyName("role_ids")]
    public List<Guid> RoleIds { get; set; } = new();
}
