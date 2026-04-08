using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class UserTests
{
    private static readonly string HashedPassword = BCrypt.Net.BCrypt.HashPassword("testPassword1");

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var createdBy = Guid.NewGuid();
        var user = new User("admin", "管理者", "admin@example.com", HashedPassword, true, createdBy, createdBy);

        user.LoginId.Should().Be("admin");
        user.UserName.Should().Be("管理者");
        user.Email.Should().Be("admin@example.com");
        user.PasswordHash.Should().Be(HashedPassword);
        user.IsActive.Should().BeTrue();
        user.CreatedBy.Should().Be(createdBy);
        user.UpdatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmptyEmailToNull()
    {
        var user = new User("admin", "管理者", "", HashedPassword, true, null, null);

        user.Email.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldNormalizeWhitespaceEmailToNull()
    {
        var user = new User("admin", "管理者", "   ", HashedPassword, true, null, null);

        user.Email.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldAllowNullEmail()
    {
        var user = new User("admin", "管理者", null, HashedPassword, true, null, null);

        user.Email.Should().BeNull();
    }

    [Fact]
    public void UserRoles_ShouldBeEmptyByDefault()
    {
        var user = new User("admin", "管理者", null, HashedPassword, true, null, null);

        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_ShouldModifyProperties()
    {
        var user = new User("admin", "管理者", "admin@example.com", HashedPassword, true, null, null);
        var actorId = Guid.NewGuid();

        user.UpdateProfile("admin2", "新管理者", "new@example.com", actorId);

        user.LoginId.Should().Be("admin2");
        user.UserName.Should().Be("新管理者");
        user.Email.Should().Be("new@example.com");
        user.UpdatedBy.Should().Be(actorId);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateTimestamp()
    {
        var user = new User("admin", "管理者", null, HashedPassword, true, null, null);
        var before = user.UpdatedAt;

        user.UpdateProfile("admin", "管理者", null, Guid.NewGuid());

        user.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void UpdateProfile_ShouldNotChangePasswordOrIsActive()
    {
        var user = new User("admin", "管理者", null, HashedPassword, true, null, null);

        user.UpdateProfile("admin2", "新管理者", null, Guid.NewGuid());

        user.PasswordHash.Should().Be(HashedPassword);
        user.IsActive.Should().BeTrue();
    }
}
