using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Template.Domain.Users;

namespace Stargazer.Orleans.Template.EntityFrameworkCore
{
    public static class DbContextModelCreatingExtensions
    {
        public static void Configure(this ModelBuilder builder)
        {
            builder.AccountConfigure();
        }

        private static void AccountConfigure(this ModelBuilder builder)
        {
            builder.Entity<Account>(b =>
            {
                b.ToTable(nameof(Account));
                b.HasKey(o => o.Id);
                b.HasIndex(o => o.AccountName).IsUnique();
            });
            
            builder.Entity<UserData>(b =>
            {
                b.ToTable(nameof(UserData));
                b.HasKey(o => o.Id);
                b.HasIndex(x => x.Email).IsUnique();
                b.HasIndex(x => x.PhoneNumber).IsUnique();
            });
        }
    }
}