using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class ObjectMetadataDto
{
    [Id(0)]
    public Guid Id { get; set; }
    
    [Id(1)]
    public string Key { get; set; } = "";
    
    [Id(2)]
    public string FileName { get; set; } = "";
    
    [Id(3)]
    public string ContentType { get; set; } = "";
    
    [Id(4)]
    public long Size { get; set; }
    
    [Id(5)]
    public string ETag { get; set; } = "";
    
    [Id(6)]
    public DateTime? LastModified { get; set; }
    
    [Id(7)]
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    [Id(8)]
    public DateTime CreationTime { get; set; }
}
