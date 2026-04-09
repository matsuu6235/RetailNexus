using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class AreaRepository : IAreaRepository
{
    private readonly RetailNexusDbContext _db;

    public AreaRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<Area?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Areas.FirstOrDefaultAsync(x => x.AreaId == id, ct);

    public Task<Area?> GetByCodeAsync(string code, CancellationToken ct)
        => _db.Areas.FirstOrDefaultAsync(x => x.AreaCode == code, ct);

    public Task<int> CountAsync(string? code, string? name, bool? isActive, CancellationToken ct)
        => BuildQuery(code, name, isActive).CountAsync(ct);

    public async Task<IReadOnlyList<Area>> ListAsync(string? code, string? name, bool? isActive, int skip, int take, CancellationToken ct)
    {
        return await BuildQuery(code, name, isActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.AreaCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Area>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        return await _db.Areas
            .Where(x => ids.Contains(x.AreaId))
            .ToListAsync(ct);
    }

    public async Task<int> GetNextDisplayOrderAsync(CancellationToken ct)
    {
        var maxOrder = await _db.Areas
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync(ct);

        return (maxOrder ?? 0) + 1;
    }

    public Task AddAsync(Area entity, CancellationToken ct)
        => _db.Areas.AddAsync(entity, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private IQueryable<Area> BuildQuery(string? code, string? name, bool? isActive)
    {
        var q = _db.Areas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
            q = q.Where(x => x.AreaCode.Contains(code));

        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => x.AreaName.Contains(name));

        if (isActive.HasValue)
            q = q.Where(x => x.IsActive == isActive.Value);

        return q;
    }
}