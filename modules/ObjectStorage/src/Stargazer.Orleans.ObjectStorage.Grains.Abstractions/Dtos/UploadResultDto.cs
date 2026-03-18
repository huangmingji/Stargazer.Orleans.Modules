using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class UploadResultDto
{
    [Id(0)]
    public string Key { get; set; } = "";
    
    [Id(1)]
    public string ETag { get; set; } = "";
    
    [Id(2)]
    public long Size { get; set; }
    
    [Id(3)]
    public string ContentType { get; set; } = "";
    
    [Id(4)]
    public DateTime LastModified { get; set; }
    
    [Id(5)]
    public string Url { get; set; } = "";
}
