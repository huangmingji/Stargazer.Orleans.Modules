using Microsoft.EntityFrameworkCore;
using Stargazer.Orleans.Users.Domain.Users;

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL
{
    public static class DbContextModelCreatingExtensions
    {
        public static void Configure(this ModelBuilder builder)
        {
            builder.Entity<UserData>(b =>
            {
                b.ToTable(nameof(UserData));
                b.HasKey(o => o.Id);
                b.HasIndex(x => x.Account).IsUnique();
                b.HasIndex(x => x.Email).IsUnique();
                b.HasIndex(x => x.PhoneNumber).IsUnique();
            });
        }
    }
}