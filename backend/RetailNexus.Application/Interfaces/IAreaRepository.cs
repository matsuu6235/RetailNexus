using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IAreaRepository
{
    Task<Area?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Area?> GetByCodeAsync(string code, CancellationToken ct);
    Task<int> CountAsync(string? code, string? name, bool? isActive, CancellationToken ct);
    Task<IReadOnlyList<Area>> ListAsync(string? code, string? name, bool? isActive, int skip, int take, CancellationToken ct);
    Task<IReadOnlyList<Area>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    Task<int> GetNextDisplayOrderAsync(CancellationToken ct);
    Task AddAsync(Area entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}