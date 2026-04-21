namespace Stargazer.Orleans.WechatManagement.Silo;

public class OrleansOptions
{
    public string ClusterId { get; set; } = "stargazer";
    public string ServiceId { get; set; } = "stargazer-app";
    public int SiloPort { get; set; } = 0;
    public int GatewayPort { get; set; } = 0;
}
