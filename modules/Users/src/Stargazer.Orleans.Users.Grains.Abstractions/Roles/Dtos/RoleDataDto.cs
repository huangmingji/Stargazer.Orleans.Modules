using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

[GenerateSerializer]
public class RoleDataDto
{
    [Id(0)]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(2)]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [Id(3)]
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }

    [Id(4)]
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [Id(5)]
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [Id(6)]
    [JsonPropertyName("permissions")]
    public List<PermissionDataDto> Permissions { get; set; } = new();

    [Id(7)]
    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}
