using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly RetailNexusDbContext _db;

    public ProductRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Product?> GetByProductCodeAsync(string productCode, CancellationToken ct)
        => _db.Products.FirstOrDefaultAsync(x => x.ProductCode == productCode, ct);

    public Task<Product?> GetByProductCodeExcludingAsync(string productCode, Guid excludeId, CancellationToken ct)
        => _db.Products.FirstOrDefaultAsync(x => x.ProductCode == productCode && x.Id != excludeId, ct);

    public async Task<IReadOnlyList<Product>> ListAsync(
        string? productCode,
        string? janCode,
        string? productName,
        string? productCategoryCode,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct)
    {
        return await BuildQuery(productCode, janCode, productName, productCategoryCode, isActive)
            .OrderBy(x => x.ProductCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        string? productCode,
        string? janCode,
        string? productName,
        string? categoryCode,
        bool? isActive,
        CancellationToken ct)
    {
        return BuildQuery(productCode, janCode, productName, categoryCode, isActive)
            .CountAsync(ct);
    }

    public async Task<string?> GetMaxProductCodeByPrefixAsync(string prefix, CancellationToken ct)
    {
        var pattern = prefix + "-";
        return await _db.Products
            .Where(x => x.ProductCode.StartsWith(pattern))
            .OrderByDescending(x => x.ProductCode)
            .Select(x => x.ProductCode)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct)
        => await _db.Products.AddAsync(product, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
    
    private IQueryable<Product> BuildQuery(
        string? productCode,
        string? janCode,
        string? productName,
        string? productCategoryCode,
        bool? isActive)
    {
        var q = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(productCode))
            q = q.Where(x => x.ProductCode.Contains(productCode));

        if (!string.IsNullOrWhiteSpace(janCode))
            q = q.Where(x => x.JanCode.Contains(janCode));

        if (!string.IsNullOrWhiteSpace(productName))
            q = q.Where(x => x.ProductName.Contains(productName));

        if (!string.IsNullOrWhiteSpace(productCategoryCode))
            q = q.Where(x => x.ProductCategoryCode == productCategoryCode);

        if (isActive.HasValue)
        {
            q = q.Where(x => x.IsActive == isActive.Value);
        }

        return q;
    }
}