using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class UploadResultDto
{
    [Id(0)] [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [Id(1)] [JsonPropertyName("etag")]
    public string ETag { get; set; } = "";

    [Id(2)] [JsonPropertyName("size")]
    public long Size { get; set; }

    [Id(3)] [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "";

    [Id(4)] [JsonPropertyName("last_modified")]
    public DateTime LastModified { get; set; }

    [Id(5)] [JsonPropertyName("url")]
    public string Url { get; set; } = "";
}
