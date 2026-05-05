using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class BucketDto
{
    [Id(0)] [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)] [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(2)] [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [Id(3)] [JsonPropertyName("acl")]
    public string Acl { get; set; } = "Private";

    [Id(4)] [JsonPropertyName("max_object_size")]
    public long MaxObjectSize { get; set; }

    [Id(5)] [JsonPropertyName("max_object_count")]
    public long MaxObjectCount { get; set; }

    [Id(6)] [JsonPropertyName("current_object_count")]
    public long CurrentObjectCount { get; set; }

    [Id(7)] [JsonPropertyName("current_storage_size")]
    public long CurrentStorageSize { get; set; }

    [Id(8)] [JsonPropertyName("owner_id")]
    public Guid OwnerId { get; set; }

    [Id(9)] [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [Id(10)] [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}
