namespace Stargazer.Orleans.Users.Domain.Users;

public sealed class UserData : Entity<Guid>
{
    public string Account { get; set; } = "";

    public string Password { get; set; } = "";

    public string SecretKey { get; set; } = "";
    
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public string Avatar { get; set; } = "";
    
    public bool IsActive { get; set; } = true;
    
    public List<UserRoles.UserRoleData> UserRoles { get; set; } = new();
}
