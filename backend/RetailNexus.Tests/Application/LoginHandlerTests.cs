using FluentAssertions;
using Moq;
using RetailNexus.Application.Features.Auth.Login;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _passwordHasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash));

        _handler = new LoginHandler(_userRepoMock.Object, _jwtServiceMock.Object, _passwordHasherMock.Object);
    }

    private static User CreateActiveUser(string loginId = "admin", string? password = null)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password ?? "password123");
        return new User(loginId, "管理者", "admin@example.com", hash, true, null, null);
    }

    private void SetupMocks(User user)
    {
        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync(user.LoginId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock
            .Setup(r => r.GetRoleNamesAsync(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Admin" });
        _userRepoMock
            .Setup(r => r.GetPermissionCodesAsync(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "products.view" });

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(60);
        _jwtServiceMock
            .Setup(j => j.CreateAccessToken(
                user,
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<DateTimeOffset>(),
                out expiresAt))
            .Returns("test-jwt-token");
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = CreateActiveUser();
        SetupMocks(user);

        var command = new LoginCommand("admin", "password123");
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.AccessToken.Should().Be("test-jwt-token");
        result.Email.Should().Be("admin@example.com");
        result.Roles.Should().Contain("Admin");
        result.Permissions.Should().Contain("products.view");
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldUpdateLastLoginAt()
    {
        var user = CreateActiveUser();
        SetupMocks(user);

        var command = new LoginCommand("admin", "password123");
        await _handler.HandleAsync(command, CancellationToken.None);

        user.LastLoginAt.Should().NotBeNull();
        _userRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldTrimLoginId()
    {
        var user = CreateActiveUser();
        SetupMocks(user);

        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand("  admin  ", "password123");
        await _handler.HandleAsync(command, CancellationToken.None);

        _userRepoMock.Verify(r => r.FindByLoginIdAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrowUnauthorized()
    {
        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("unknown", "password");

        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task HandleAsync_WhenUserInactive_ShouldThrowUnauthorized()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var inactiveUser = new User("admin", "管理者", null, hash, false, null, null);
        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        var command = new LoginCommand("admin", "password123");

        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordIncorrect_ShouldThrowUnauthorized()
    {
        var user = CreateActiveUser();
        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand("admin", "wrong-password");

        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordIncorrect_ShouldNotUpdateLastLoginAt()
    {
        var user = CreateActiveUser();
        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand("admin", "wrong-password");

        try { await _handler.HandleAsync(command, CancellationToken.None); }
        catch (UnauthorizedAccessException) { }

        user.LastLoginAt.Should().BeNull();
        _userRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNullEmail_ShouldReturnEmptyString()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User("admin", "管理者", null, hash, true, null, null);
        SetupMocks(user);

        _userRepoMock
            .Setup(r => r.FindByLoginIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand("admin", "password123");
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Email.Should().BeEmpty();
    }
}
