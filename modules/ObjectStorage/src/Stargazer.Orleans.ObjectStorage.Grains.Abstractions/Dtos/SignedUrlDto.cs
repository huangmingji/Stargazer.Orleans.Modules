using Orleans;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

[GenerateSerializer]
public class SignedUrlDto
{
    [Id(0)]
    public string Url { get; set; } = "";
    
    [Id(1)]
    public DateTime ExpiresAt { get; set; }
    
    [Id(2)]
    public string Method { get; set; } = "GET";
}
