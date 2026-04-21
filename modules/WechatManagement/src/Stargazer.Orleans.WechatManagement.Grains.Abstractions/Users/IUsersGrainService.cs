using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IUsersGrainService : IGrainWithStringKey
{
    Task<List<UserDataDto>> GetUsersByPage(CancellationToken cancellationToken = default);
}
