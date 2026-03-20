using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly RetailNexusDbContext _db;

    public ProductCategoryRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.ProductCategories.FirstOrDefaultAsync(x => x.ProductCategoryId == id, ct);

    public Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken ct)
        => _db.ProductCategories.FirstOrDefaultAsync(x => x.ProductCategoryCd == code, ct);

    public Task<int> CountAsync(string? code, string? name, bool? isActive, CancellationToken ct)
        => BuildQuery(code, name, isActive).CountAsync(ct);

    public async Task<IReadOnlyList<ProductCategory>> ListAsync(
        string? code,
        string? name,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct)
    {
        return await BuildQuery(code, name, isActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ProductCategoryCd)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
    
    public async Task<IReadOnlyList<ProductCategory>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        return await _db.ProductCategories
            .Where(x => ids.Contains(x.ProductCategoryId))
            .ToListAsync(ct);
    }

    public async Task<int> GetNextDisplayOrderAsync(CancellationToken ct)
    {
        var maxOrder = await _db.ProductCategories
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync(ct);

        return (maxOrder ?? 0) + 1;
    }

    public Task AddAsync(ProductCategory entity, CancellationToken ct)
        => _db.ProductCategories.AddAsync(entity, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private IQueryable<ProductCategory> BuildQuery(string? code, string? name, bool? isActive)
    {
        var q = _db.ProductCategories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
            q = q.Where(x => x.ProductCategoryCd.Contains(code.Trim()));

        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => x.ProductCategoryName.Contains(name.Trim()));

        if (isActive.HasValue)
            q = q.Where(x => x.IsActive == isActive.Value);

        return q;
    }
}