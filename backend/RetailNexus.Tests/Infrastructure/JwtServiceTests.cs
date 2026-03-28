using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Security;

namespace RetailNexus.Tests.Infrastructure;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _config;

    private static readonly IReadOnlyList<string> DefaultRoles = new[] { "Admin" };
    private static readonly IReadOnlyList<string> DefaultPermissions = new[] { "products.view", "products.create" };

    public JwtServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "RetailNexus",
            ["Jwt:Audience"] = "RetailNexusApp",
            ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForTestingPurposes123456",
            ["Jwt:AccessTokenMinutes"] = "60"
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _jwtService = new JwtService(_config);
    }

    private static User CreateUser()
    {
        return new User("admin", "管理者", "admin@example.com", "password", true, null, null);
    }

    [Fact]
    public void CreateAccessToken_ShouldReturnValidJwtString()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void CreateAccessToken_ShouldSetCorrectExpiration()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out var expiresAt);

        expiresAt.Should().BeCloseTo(now.AddMinutes(60), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateAccessToken_ShouldContainUserIdClaim()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UserId.ToString());
    }

    [Fact]
    public void CreateAccessToken_ShouldContainEmailClaim()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "admin@example.com");
    }

    [Fact]
    public void CreateAccessToken_ShouldContainRoleClaim()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void CreateAccessToken_ShouldContainPermissionClaims()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "products.view");
        jwt.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "products.create");
    }

    [Fact]
    public void CreateAccessToken_ShouldSetCorrectIssuerAndAudience()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("RetailNexus");
        jwt.Audiences.Should().Contain("RetailNexusApp");
    }

    [Fact]
    public void CreateAccessToken_ShouldUseHmacSha256()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Header.Alg.Should().Be("HS256");
    }

    [Fact]
    public void CreateAccessToken_ShouldSetNotBeforeToNow()
    {
        var user = CreateUser();
        var now = DateTimeOffset.UtcNow;

        var token = _jwtService.CreateAccessToken(user, DefaultRoles, DefaultPermissions, now, out _);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidFrom.Should().BeCloseTo(now.UtcDateTime, TimeSpan.FromSeconds(1));
    }
}
