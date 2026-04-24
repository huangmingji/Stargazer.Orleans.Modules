using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UpdateUserStatusInputDto
{
    [Id(0)]
    [JsonPropertyName("is_enabled")]
    public bool IsEnabled { get; set; }
}
