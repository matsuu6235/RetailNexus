using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly RetailNexusDbContext _db;

    public InventoryRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<Inventory?> GetByProductAndStoreAsync(Guid productId, Guid storeId, CancellationToken ct)
        => _db.Inventories
            .Include(x => x.Product)
            .Include(x => x.Store)
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StoreId == storeId, ct);

    public async Task<IReadOnlyList<Inventory>> ListAsync(
        Guid? areaId,
        Guid? storeId,
        Guid? productCategoryId,
        string? productCode,
        Guid? supplierId,
        string? stockStatus,
        int skip,
        int take,
        CancellationToken ct)
    {
        return await BuildQuery(areaId, storeId, productCategoryId, productCode, supplierId, stockStatus)
            .Include(x => x.Product)
            .Include(x => x.Store)
                .ThenInclude(s => s!.Area)
            .OrderBy(x => x.Store!.StoreName)
            .ThenBy(x => x.Product!.ProductCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        Guid? areaId,
        Guid? storeId,
        Guid? productCategoryId,
        string? productCode,
        Guid? supplierId,
        string? stockStatus,
        CancellationToken ct)
    {
        return BuildQuery(areaId, storeId, productCategoryId, productCode, supplierId, stockStatus)
            .CountAsync(ct);
    }

    public async Task AddAsync(Inventory inventory, CancellationToken ct)
        => await _db.Inventories.AddAsync(inventory, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private IQueryable<Inventory> BuildQuery(
        Guid? areaId,
        Guid? storeId,
        Guid? productCategoryId,
        string? productCode,
        Guid? supplierId,
        string? stockStatus)
    {
        var q = _db.Inventories.AsQueryable();

        if (areaId.HasValue)
            q = q.Where(x => x.Store!.AreaId == areaId.Value);

        if (storeId.HasValue)
            q = q.Where(x => x.StoreId == storeId.Value);

        if (productCategoryId.HasValue)
        {
            var categoryCode = _db.ProductCategories
                .Where(c => c.ProductCategoryId == productCategoryId.Value)
                .Select(c => c.ProductCategoryCode)
                .FirstOrDefault();
            if (categoryCode != null)
                q = q.Where(x => x.Product!.ProductCategoryCode == categoryCode);
        }

        if (!string.IsNullOrWhiteSpace(productCode))
            q = q.Where(x => x.Product!.ProductCode.Contains(productCode));

        // supplierId フィルターは Product に SupplierId がないため未実装

        if (stockStatus == "inStock")
            q = q.Where(x => x.Quantity > 0);
        else if (stockStatus == "outOfStock")
            q = q.Where(x => x.Quantity <= 0);

        return q;
    }
}
