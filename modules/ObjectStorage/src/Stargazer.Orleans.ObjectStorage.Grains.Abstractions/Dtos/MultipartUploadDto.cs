using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class InitiateMultipartUploadResultDto
{
    [Id(0)]
    public string UploadId { get; set; } = "";
    
    [Id(1)]
    public string Key { get; set; } = "";
    
    [Id(2)]
    public string Bucket { get; set; } = "";
}

[GenerateSerializer]
public class UploadPartResultDto
{
    [Id(0)]
    public int PartNumber { get; set; }
    
    [Id(1)]
    public string ETag { get; set; } = "";
}

[GenerateSerializer]
public class CompleteMultipartUploadDto
{
    [Id(0)]
    public string UploadId { get; set; } = "";
    
    [Id(1)]
    public List<PartETagDto> Parts { get; set; } = new();
}

[GenerateSerializer]
public class PartETagDto
{
    [Id(0)]
    public int PartNumber { get; set; }
    
    [Id(1)]
    public string ETag { get; set; } = "";
}
