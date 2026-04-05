using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _repoMock = new();
    private readonly RoleService _service;

    public RoleServiceTests()
    {
        _service = new RoleService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRole_ReplacePermissions()
    {
        var actorId = Guid.NewGuid();
        var permissionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _repoMock.Setup(r => r.FindByNameAsync("管理者", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        _repoMock.Setup(r => r.FindByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Role("管理者", "管理者ロール", actorId));

        var result = await _service.CreateAsync("管理者", "管理者ロール", true, permissionIds, actorId, CancellationToken.None);

        result.RoleName.Should().Be("管理者");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.ReplaceRolePermissionsAsync(It.IsAny<Guid>(), permissionIds, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.FindByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameEmpty_ShouldThrowBusinessRuleException()
    {
        var act = () => _service.CreateAsync("", null, true, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateName_ShouldThrowDuplicateException()
    {
        var existingRole = new Role("管理者", "既存ロール");
        _repoMock.Setup(r => r.FindByNameAsync("管理者", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        var act = () => _service.CreateAsync("管理者", "新しいロール", true, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRole_ReplacePermissions()
    {
        var actorId = Guid.NewGuid();
        var role = new Role("管理者", "管理者ロール", actorId);
        var permissionIds = new List<Guid> { Guid.NewGuid() };

        _repoMock.Setup(r => r.FindByIdWithPermissionsAsync(role.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _repoMock.Setup(r => r.FindByNameAsync("更新ロール", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await _service.UpdateAsync(role.RoleId, "更新ロール", "更新説明", permissionIds, actorId, CancellationToken.None);

        role.RoleName.Should().Be("更新ロール");
        role.Description.Should().Be("更新説明");
        _repoMock.Verify(r => r.ReplaceRolePermissionsAsync(role.RoleId, permissionIds, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.FindByIdWithPermissionsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var act = () => _service.UpdateAsync(id, "テスト", null, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldSetIsActive()
    {
        var actorId = Guid.NewGuid();
        var role = new Role("管理者", "管理者ロール", actorId);

        _repoMock.Setup(r => r.FindByIdAsync(role.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        await _service.ChangeActivationAsync(role.RoleId, false, actorId, CancellationToken.None);

        role.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
