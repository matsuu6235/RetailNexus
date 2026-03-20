using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByLoginIdAsync(string loginId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}