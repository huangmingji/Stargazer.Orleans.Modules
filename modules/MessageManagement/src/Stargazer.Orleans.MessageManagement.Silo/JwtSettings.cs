namespace Stargazer.Orleans.MessageManagement.Silo;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    
    public string Issuer { get; set; } = "Stargazer.Orleans.MessageManagement";
    
    public string Audience { get; set; } = "Stargazer.Orleans.MessageManagement";
    
    public int ExpiryMinutes { get; set; } = 60;
    
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
