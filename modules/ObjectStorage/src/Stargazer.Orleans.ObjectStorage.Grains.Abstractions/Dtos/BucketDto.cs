using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class BucketDto
{
    [Id(0)]
    public Guid Id { get; set; }
    
    [Id(1)]
    public string Name { get; set; } = "";
    
    [Id(2)]
    public string Description { get; set; } = "";
    
    [Id(3)]
    public string Acl { get; set; } = "Private";
    
    [Id(4)]
    public long MaxObjectSize { get; set; }
    
    [Id(5)]
    public long MaxObjectCount { get; set; }
    
    [Id(6)]
    public long CurrentObjectCount { get; set; }
    
    [Id(7)]
    public long CurrentStorageSize { get; set; }
    
    [Id(8)]
    public Guid OwnerId { get; set; }
    
    [Id(9)]
    public bool IsActive { get; set; }
    
    [Id(10)]
    public DateTime CreationTime { get; set; }
}
