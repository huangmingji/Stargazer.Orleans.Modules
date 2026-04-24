using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions;

[GenerateSerializer]
public class PageResult<T>
{
    [Id(0)]
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [Id(1)]
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new List<T>();
}