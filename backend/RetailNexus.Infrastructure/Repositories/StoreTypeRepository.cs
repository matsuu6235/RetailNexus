using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class StoreTypeRepository : IStoreTypeRepository
{
    private readonly RetailNexusDbContext _db;

    public StoreTypeRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<StoreType?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.StoreTypes.FirstOrDefaultAsync(x => x.StoreTypeId == id, ct);

    public Task<StoreType?> GetByCodeAsync(string code, CancellationToken ct)
        => _db.StoreTypes.FirstOrDefaultAsync(x => x.StoreTypeCd == code, ct);

    public async Task<IReadOnlyList<StoreType>> ListAsync(string? code, string? name, bool? isActive, CancellationToken ct)
    {
        var q = _db.StoreTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
            q = q.Where(x => x.StoreTypeCd.Contains(code));

        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => x.StoreTypeName.Contains(name));

        if (isActive.HasValue)
            q = q.Where(x => x.IsActive == isActive.Value);

        return await q
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.StoreTypeCd)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StoreType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        return await _db.StoreTypes
            .Where(x => ids.Contains(x.StoreTypeId))
            .ToListAsync(ct);
    }

    public async Task<int> GetNextDisplayOrderAsync(CancellationToken ct)
    {
        var maxOrder = await _db.StoreTypes
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync(ct);

        return (maxOrder ?? 0) + 1;
    }

    public Task AddAsync(StoreType entity, CancellationToken ct)
        => _db.StoreTypes.AddAsync(entity, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}