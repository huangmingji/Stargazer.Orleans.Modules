using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.MessageManagement.Grains.SeedData;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.SeedData;

public class MessageManagementSeedDataInitializerTests
{
    private readonly Mock<IClusterClient> _clusterClientMock;
    private readonly Mock<IPermissionGrain> _permissionGrainMock;
    private readonly Mock<IRoleGrain> _roleGrainMock;
    private readonly Mock<ILogger<MessageManagementSeedDataInitializer>> _loggerMock;

    public MessageManagementSeedDataInitializerTests()
    {
        _clusterClientMock = new Mock<IClusterClient>();
        _permissionGrainMock = new Mock<IPermissionGrain>();
        _roleGrainMock = new Mock<IRoleGrain>();
        _loggerMock = new Mock<ILogger<MessageManagementSeedDataInitializer>>();

        _clusterClientMock.Setup(x => x.GetGrain<IPermissionGrain>(0, null)).Returns(_permissionGrainMock.Object);
        _clusterClientMock.Setup(x => x.GetGrain<IRoleGrain>(0, null)).Returns(_roleGrainMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldSeedPermissionsAndCreateRole()
    {
        _permissionGrainMock
            .Setup(x => x.GetPermissionByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PermissionDataDto?)null);

        _permissionGrainMock
            .Setup(x => x.CreatePermissionAsync(It.IsAny<PermissionDataDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissionDataDto { Id = Guid.NewGuid() });

        _permissionGrainMock
            .Setup(x => x.GetPermissionsByCategoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PermissionDataDto>
            {
                new() { Id = Guid.NewGuid(), Name = "发送消息", Code = AuthorizationPermissions.Messages.Send },
                new() { Id = Guid.NewGuid(), Name = "查看消息", Code = AuthorizationPermissions.Messages.View }
            });

        _roleGrainMock
            .Setup(x => x.GetRoleByNameAsync("MessageAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleDataDto?)null);

        _roleGrainMock
            .Setup(x => x.CreateRoleAsync(It.IsAny<CreateOrUpdateRoleInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleDataDto { Id = Guid.NewGuid(), Name = "MessageAdmin" });

        _roleGrainMock
            .Setup(x => x.AssignPermissionsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var initializer = new MessageManagementSeedDataInitializer(
            _clusterClientMock.Object,
            _loggerMock.Object);

        await initializer.Execute(CancellationToken.None);

        _permissionGrainMock.Verify(
            x => x.CreatePermissionAsync(It.Is<PermissionDataDto>(p => p.Type == 3), It.IsAny<CancellationToken>()),
            Times.Exactly(8));

        _roleGrainMock.Verify(
            x => x.CreateRoleAsync(It.Is<CreateOrUpdateRoleInputDto>(r => r.Name == "MessageAdmin" && r.Priority == 100), It.IsAny<CancellationToken>()),
            Times.Once);

        _roleGrainMock.Verify(
            x => x.AssignPermissionsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenRoleExists_ShouldAssignPermissionsOnly()
    {
        var existingRoleId = Guid.NewGuid();
        var messagePermissions = new List<PermissionDataDto>
        {
            new() { Id = Guid.NewGuid(), Code = AuthorizationPermissions.Messages.Send },
            new() { Id = Guid.NewGuid(), Code = AuthorizationPermissions.Messages.View }
        };
        var templatePermissions = new List<PermissionDataDto>
        {
            new() { Id = Guid.NewGuid(), Code = AuthorizationPermissions.Templates.Create }
        };

        _permissionGrainMock
            .Setup(x => x.GetPermissionByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissionDataDto { Id = Guid.NewGuid() });

        _permissionGrainMock
            .Setup(x => x.GetPermissionsByCategoryAsync("消息管理", It.IsAny<CancellationToken>()))
            .ReturnsAsync(messagePermissions);

        _permissionGrainMock
            .Setup(x => x.GetPermissionsByCategoryAsync("消息模板", It.IsAny<CancellationToken>()))
            .ReturnsAsync(templatePermissions);

        _roleGrainMock
            .Setup(x => x.GetRoleByNameAsync("MessageAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleDataDto { Id = existingRoleId, Name = "MessageAdmin" });

        _roleGrainMock
            .Setup(x => x.AssignPermissionsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var initializer = new MessageManagementSeedDataInitializer(
            _clusterClientMock.Object,
            _loggerMock.Object);

        await initializer.Execute(CancellationToken.None);

        _roleGrainMock.Verify(
            x => x.CreateRoleAsync(It.IsAny<CreateOrUpdateRoleInputDto>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _roleGrainMock.Verify(
            x => x.AssignPermissionsAsync(existingRoleId, It.Is<List<Guid>>(ids => ids.Count == 3), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenPermissionExists_ShouldSkipCreation()
    {
        _permissionGrainMock
            .Setup(x => x.GetPermissionByCodeAsync(AuthorizationPermissions.Messages.Send, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissionDataDto { Id = Guid.NewGuid(), Code = AuthorizationPermissions.Messages.Send });

        _permissionGrainMock
            .Setup(x => x.GetPermissionByCodeAsync(AuthorizationPermissions.Messages.View, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PermissionDataDto?)null);

        _permissionGrainMock
            .Setup(x => x.CreatePermissionAsync(It.IsAny<PermissionDataDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissionDataDto { Id = Guid.NewGuid() });

        _permissionGrainMock
            .Setup(x => x.GetPermissionsByCategoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PermissionDataDto>());

        _roleGrainMock
            .Setup(x => x.GetRoleByNameAsync("MessageAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleDataDto { Id = Guid.NewGuid() });

        var initializer = new MessageManagementSeedDataInitializer(
            _clusterClientMock.Object,
            _loggerMock.Object);

        await initializer.Execute(CancellationToken.None);

        _permissionGrainMock.Verify(
            x => x.CreatePermissionAsync(
                It.Is<PermissionDataDto>(p => p.Code == AuthorizationPermissions.Messages.Send),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _permissionGrainMock.Verify(
            x => x.CreatePermissionAsync(
                It.Is<PermissionDataDto>(p => p.Code == AuthorizationPermissions.Messages.View),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenNoPermissionsFound_ShouldNotAssignPermissions()
    {
        _permissionGrainMock
            .Setup(x => x.GetPermissionByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PermissionDataDto?)null);

        _permissionGrainMock
            .Setup(x => x.CreatePermissionAsync(It.IsAny<PermissionDataDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissionDataDto { Id = Guid.NewGuid() });

        _permissionGrainMock
            .Setup(x => x.GetPermissionsByCategoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PermissionDataDto>());

        _roleGrainMock
            .Setup(x => x.GetRoleByNameAsync("MessageAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleDataDto?)null);

        _roleGrainMock
            .Setup(x => x.CreateRoleAsync(It.IsAny<CreateOrUpdateRoleInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleDataDto { Id = Guid.NewGuid() });

        var initializer = new MessageManagementSeedDataInitializer(
            _clusterClientMock.Object,
            _loggerMock.Object);

        await initializer.Execute(CancellationToken.None);

        _roleGrainMock.Verify(
            x => x.AssignPermissionsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
