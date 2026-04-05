using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_userRepoMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPassword_CreateUser_ReplaceRoles()
    {
        var actorId = Guid.NewGuid();
        var roleIds = new List<Guid> { Guid.NewGuid() };

        _passwordHasherMock.Setup(h => h.Hash("password123")).Returns("hashed");
        _userRepoMock.Setup(r => r.FindByLoginIdAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.FindByIdWithRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new User("testuser", "テストユーザー", "test@example.com", "hashed", true, actorId, actorId));

        var result = await _service.CreateAsync("testuser", "テストユーザー", "test@example.com", "password123", true, roleIds, actorId, CancellationToken.None);

        result.LoginId.Should().Be("testuser");
        _passwordHasherMock.Verify(h => h.Hash("password123"), Times.Once);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.ReplaceUserRolesAsync(It.IsAny<Guid>(), roleIds, It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.FindByIdWithRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenLoginIdEmpty_ShouldThrowBusinessRuleException()
    {
        var act = () => _service.CreateAsync("", "テストユーザー", null, "password123", true, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task CreateAsync_WhenPasswordTooShort_ShouldThrowBusinessRuleException()
    {
        var act = () => _service.CreateAsync("testuser", "テストユーザー", null, "short", true, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateLoginId_ShouldThrowDuplicateException()
    {
        var existingUser = new User("testuser", "既存ユーザー", null, "hash", true, null, null);
        _userRepoMock.Setup(r => r.FindByLoginIdAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var act = () => _service.CreateAsync("testuser", "テストユーザー", null, "password123", true, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProfile_ReplaceRoles()
    {
        var actorId = Guid.NewGuid();
        var user = new User("testuser", "テストユーザー", "test@example.com", "hash", true, actorId, actorId);
        var roleIds = new List<Guid> { Guid.NewGuid() };

        _userRepoMock.Setup(r => r.FindByIdWithRolesAsync(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.FindByLoginIdAsync("updateduser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _service.UpdateAsync(user.UserId, "updateduser", "更新ユーザー", "updated@example.com", roleIds, actorId, CancellationToken.None);

        user.LoginId.Should().Be("updateduser");
        user.UserName.Should().Be("更新ユーザー");
        _userRepoMock.Verify(r => r.ReplaceUserRolesAsync(user.UserId, roleIds, It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _userRepoMock.Setup(r => r.FindByIdWithRolesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _service.UpdateAsync(id, "testuser", "テストユーザー", null, new List<Guid>(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldHashAndSetPassword()
    {
        var actorId = Guid.NewGuid();
        var user = new User("testuser", "テストユーザー", null, "oldhash", true, actorId, actorId);

        _userRepoMock.Setup(r => r.FindByIdAsync(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Hash("newpassword123")).Returns("newhash");

        await _service.ResetPasswordAsync(user.UserId, "newpassword123", actorId, CancellationToken.None);

        user.PasswordHash.Should().Be("newhash");
        _passwordHasherMock.Verify(h => h.Hash("newpassword123"), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenPasswordTooShort_ShouldThrowBusinessRuleException()
    {
        var act = () => _service.ResetPasswordAsync(Guid.NewGuid(), "short", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
