using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

[GenerateSerializer]
public class PermissionDataDto
{
    [Id(0)]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(2)]
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [Id(3)]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [Id(4)]
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [Id(5)]
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [Id(6)]
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
}
