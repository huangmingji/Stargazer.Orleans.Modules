using Orleans;

namespace Stargazer.Orleans.Template.Domain.Users;

[Serializable]
public class UserData : Entity<Guid>
{
    public Guid? AccountId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string Avatar { get; set; }
}