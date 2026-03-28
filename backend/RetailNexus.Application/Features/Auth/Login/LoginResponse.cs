namespace RetailNexus.Application.Features.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string Email,
    string[] Roles,
    string[] Permissions
);
