using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IJwtService
{
    string CreateAccessToken(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions, DateTimeOffset now, out DateTimeOffset expiresAt);
}