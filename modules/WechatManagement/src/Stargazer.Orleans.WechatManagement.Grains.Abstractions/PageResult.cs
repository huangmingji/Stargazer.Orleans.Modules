namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions;

[GenerateSerializer]
public class PageResult<T>
{
    [Id(0)]
    public int Total { get; set; }

    [Id(1)]
    public List<T> Items { get; set; } = new List<T>();
}