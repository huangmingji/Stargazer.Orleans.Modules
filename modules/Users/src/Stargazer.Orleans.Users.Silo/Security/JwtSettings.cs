namespace Stargazer.Orleans.Users.Silo.Security;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    
    public string Issuer { get; set; } = "Stargazer.Orleans.Users";
    
    public string Audience { get; set; } = "Stargazer.Orleans.Users";
    
    public int ExpiryMinutes { get; set; } = 60;
    
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
