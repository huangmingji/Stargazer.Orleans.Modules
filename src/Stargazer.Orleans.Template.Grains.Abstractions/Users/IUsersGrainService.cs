using Orleans;
using Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Template.Grains.Abstractions.Users;

public interface IUsersGrainService : IGrainWithStringKey
{
    Task<List<UserDataDto>> GetUsersByPage(CancellationToken cancellationToken = default);
}