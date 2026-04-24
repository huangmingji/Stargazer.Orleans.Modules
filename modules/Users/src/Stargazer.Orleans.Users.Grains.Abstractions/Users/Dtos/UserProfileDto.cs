using System.Text.Json.Serialization;
using Orleans;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UserProfileDto
{
    [Id(0)]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)]
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [Id(2)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(3)]
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [Id(4)]
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = "";

    [Id(5)]
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = "";

    [Id(6)]
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Id(7)]
    [JsonPropertyName("roles")]
    public List<RoleDataDto> Roles { get; set; } = new();
    
    [Id(8)]
    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}
