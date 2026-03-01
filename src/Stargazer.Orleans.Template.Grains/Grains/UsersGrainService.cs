using Orleans.Concurrency;
using Stargazer.Orleans.Template.Domain.Users;
using Stargazer.Orleans.Template.EntityFrameworkCore.Repositories;
using Stargazer.Orleans.Template.Grains.Abstractions.Users;
using Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Template.Grains.Grains;

[StatelessWorker]
public class UsersGrainService : Grain, IUsersGrainService
{
    private IRepository<UserData, Guid> _userRepository;
    public Task<List<UserDataDto>> GetUsersByPage(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}