using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class ObjectMetadataDto
{
    [Id(0)] [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)] [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [Id(2)] [JsonPropertyName("file_name")]
    public string FileName { get; set; } = "";

    [Id(3)] [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "";

    [Id(4)] [JsonPropertyName("size")]
    public long Size { get; set; }

    [Id(5)] [JsonPropertyName("etag")]
    public string ETag { get; set; } = "";

    [Id(6)] [JsonPropertyName("last_modified")]
    public DateTime? LastModified { get; set; }

    [Id(7)] [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();

    [Id(8)] [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}
