using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IJwtService
{
    string CreateAccessToken(User user, DateTimeOffset now, out DateTimeOffset expiresAt);
}