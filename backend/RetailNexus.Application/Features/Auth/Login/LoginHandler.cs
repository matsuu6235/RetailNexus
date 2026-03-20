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

        // 現時点では仮で文字列比較。後でBCrypt等に置き換える
        if (user.PasswordHash != command.Password)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var now = DateTimeOffset.UtcNow;
        user.LastLoginAt = now;
        user.UpdatedAt = now;
        user.UpdatedBy = user.UserId;

        await _users.SaveChangesAsync(ct);

        var token = _jwt.CreateAccessToken(user, now, out var expiresAt);

        return new LoginResponse(
            token,
            expiresAt,
            user.Email ?? string.Empty,
            user.Role
        );
    }
}