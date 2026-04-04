using RetailNexus.Application.Interfaces;

namespace RetailNexus.Application.Features.Auth.Login;

public class LoginHandler
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _passwordHasher;

    public LoginHandler(IUserRepository users, IJwtService jwt, IPasswordHasher passwordHasher)
    {
        _users = users;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken ct)
    {
        var loginId = command.LoginId.Trim();
        var user = await _users.FindByLoginIdAsync(loginId, ct);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var now = DateTimeOffset.UtcNow;
        user.LastLoginAt = now;

        await _users.SaveChangesAsync(ct);

        var roles = await _users.GetRoleNamesAsync(user.UserId, ct);
        var permissions = await _users.GetPermissionCodesAsync(user.UserId, ct);

        var token = _jwt.CreateAccessToken(user, roles, permissions, now, out var expiresAt);

        return new LoginResponse(
            token,
            expiresAt,
            user.Email ?? string.Empty,
            roles.ToArray(),
            permissions.ToArray()
        );
    }
}
