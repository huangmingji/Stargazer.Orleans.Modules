using System.Security;
using Orleans;
using Stargazer.Orleans.Utility.Cryptography;
using Stargazer.Orleans.Utility.ExceptionExtensions;

namespace Stargazer.Orleans.Template.Domain.Users;

[Serializable]
public class Account
{
    public Guid Id { get; set; }
    
    public string AccountName { get; set; }

    public string Password { get; set; }

    public string SaltKey { get; set; }

    public Guid CreatorId { get; set; }
    
    public DateTime CreateTime { get; set; } = DateTime.Now;

    public Guid? LastModifierId { get; set; }

    public DateTime? LastModifyTime { get; set; }
}