using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class SignedUrlDto
{
    [Id(0)] [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [Id(1)] [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Id(2)] [JsonPropertyName("method")]
    public string Method { get; set; } = "GET";
}
