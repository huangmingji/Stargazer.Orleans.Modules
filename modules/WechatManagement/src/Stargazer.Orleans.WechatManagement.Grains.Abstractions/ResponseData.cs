namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions;

[GenerateSerializer]
public class ResponseData
{
    [Id(0)] public string Code { get; set; } = "";

    [Id(1)] public string Message { get; set; } = "";

    [Id(2)]
    public object? Data { get; set; }
    
    public static ResponseData Success(string code = "success", string message = "success", object? data = null)
    {
        return new ResponseData
        {
            Code = code,
            Message = message,
            Data = data
        };
    }
    
    public static ResponseData Fail(string code = "fail", string message = "")
    {
        return new ResponseData
        {
            Code = code,
            Message = message
        };
    }
}