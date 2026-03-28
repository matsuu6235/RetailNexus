using RetailNexus.Application.Interfaces;

namespace RetailNexus.Application.Features.Auth.Login;

public class LoginHandler
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public LoginHandler(IUserRepository users, IJwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken ct)
    {
        var loginId = command.LoginId.Trim();
        var user = await _users.FindByLoginIdAsync(loginId, ct);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var now = DateTimeOffset.UtcNow;
        user.LastLoginAt = now;
        user.UpdatedAt = now;
        user.UpdatedBy = user.UserId;

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
