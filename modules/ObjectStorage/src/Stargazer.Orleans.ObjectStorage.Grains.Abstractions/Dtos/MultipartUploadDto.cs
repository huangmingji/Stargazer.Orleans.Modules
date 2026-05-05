using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class InitiateMultipartUploadResultDto
{
    [Id(0)] [JsonPropertyName("upload_id")]
    public string UploadId { get; set; } = "";

    [Id(1)] [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [Id(2)] [JsonPropertyName("bucket")]
    public string Bucket { get; set; } = "";
}

[GenerateSerializer]
public class UploadPartResultDto
{
    [Id(0)] [JsonPropertyName("part_number")]
    public int PartNumber { get; set; }

    [Id(1)] [JsonPropertyName("etag")]
    public string ETag { get; set; } = "";
}

[GenerateSerializer]
public class CompleteMultipartUploadDto
{
    [Id(0)] [JsonPropertyName("upload_id")]
    public string UploadId { get; set; } = "";

    [Id(1)] [JsonPropertyName("parts")]
    public List<PartETagDto> Parts { get; set; } = new();
}

[GenerateSerializer]
public class PartETagDto
{
    [Id(0)] [JsonPropertyName("part_number")]
    public int PartNumber { get; set; }

    [Id(1)] [JsonPropertyName("etag")]
    public string ETag { get; set; } = "";
}
