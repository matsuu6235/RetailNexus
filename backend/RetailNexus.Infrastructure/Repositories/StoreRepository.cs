using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class StoreRepository : IStoreRepository
{
    private readonly RetailNexusDbContext _db;

    public StoreRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<Store?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Stores
            .Include(x => x.Area)
            .Include(x => x.StoreType)
            .FirstOrDefaultAsync(x => x.StoreId == id, ct);

    public Task<Store?> GetByCodeAsync(string code, CancellationToken ct)
        => _db.Stores.FirstOrDefaultAsync(x => x.StoreCd == code, ct);

    public Task<int> CountAsync(string? code, string? name, Guid? areaId, Guid? storeTypeId, bool? isActive, CancellationToken ct)
        => BuildQuery(code, name, areaId, storeTypeId, isActive).CountAsync(ct);

    public async Task<IReadOnlyList<Store>> ListAsync(string? code, string? name, Guid? areaId, Guid? storeTypeId, bool? isActive, int skip, int take, CancellationToken ct)
    {
        return await BuildQuery(code, name, areaId, storeTypeId, isActive)
            .OrderBy(x => x.StoreCd)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<string?> GetMaxStoreCodeAsync(CancellationToken ct)
    {
        return await _db.Stores
            .OrderByDescending(x => x.StoreCd)
            .Select(x => x.StoreCd)
            .FirstOrDefaultAsync(ct);
    }

    public Task AddAsync(Store entity, CancellationToken ct)
        => _db.Stores.AddAsync(entity, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private IQueryable<Store> BuildQuery(string? code, string? name, Guid? areaId, Guid? storeTypeId, bool? isActive)
    {
        var q = _db.Stores
            .Include(x => x.Area)
            .Include(x => x.StoreType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
            q = q.Where(x => x.StoreCd.Contains(code));

        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => x.StoreName.Contains(name));

        if (areaId.HasValue)
            q = q.Where(x => x.AreaId == areaId.Value);

        if (storeTypeId.HasValue)
            q = q.Where(x => x.StoreTypeId == storeTypeId.Value);

        if (isActive.HasValue)
            q = q.Where(x => x.IsActive == isActive.Value);

        return q;
    }
}