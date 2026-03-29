using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly RetailNexusDbContext _db;

    public PurchaseOrderRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.PurchaseOrders
            .Include(x => x.Supplier)
            .Include(x => x.Store)
            .Include(x => x.Approver)
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == id, ct);

    public Task<PurchaseOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct)
        => _db.PurchaseOrders
            .Include(x => x.Supplier)
            .Include(x => x.Store)
            .Include(x => x.Approver)
            .Include(x => x.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == id, ct);

    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(
        string? orderNumber,
        Guid? supplierId,
        Guid? storeId,
        PurchaseOrderStatus? status,
        DateTimeOffset? orderDateFrom,
        DateTimeOffset? orderDateTo,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct)
    {
        return await BuildQuery(orderNumber, supplierId, storeId, status, orderDateFrom, orderDateTo, isActive)
            .Include(x => x.Supplier)
            .Include(x => x.Store)
            .Include(x => x.Approver)
            .OrderByDescending(x => x.OrderDate)
            .ThenByDescending(x => x.OrderNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        string? orderNumber,
        Guid? supplierId,
        Guid? storeId,
        PurchaseOrderStatus? status,
        DateTimeOffset? orderDateFrom,
        DateTimeOffset? orderDateTo,
        bool? isActive,
        CancellationToken ct)
    {
        return BuildQuery(orderNumber, supplierId, storeId, status, orderDateFrom, orderDateTo, isActive)
            .CountAsync(ct);
    }

    public async Task<string?> GetMaxOrderNumberAsync(CancellationToken ct)
    {
        return await _db.PurchaseOrders
            .OrderByDescending(x => x.OrderNumber)
            .Select(x => x.OrderNumber)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(PurchaseOrder order, CancellationToken ct)
        => await _db.PurchaseOrders.AddAsync(order, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    public void RemoveDetails(IEnumerable<PurchaseOrderDetail> details)
        => _db.PurchaseOrderDetails.RemoveRange(details);

    public void AddDetail(PurchaseOrderDetail detail)
        => _db.PurchaseOrderDetails.Add(detail);

    private IQueryable<PurchaseOrder> BuildQuery(
        string? orderNumber,
        Guid? supplierId,
        Guid? storeId,
        PurchaseOrderStatus? status,
        DateTimeOffset? orderDateFrom,
        DateTimeOffset? orderDateTo,
        bool? isActive)
    {
        var q = _db.PurchaseOrders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var value = orderNumber.Trim();
            q = q.Where(x => x.OrderNumber.Contains(value));
        }

        if (supplierId.HasValue)
            q = q.Where(x => x.SupplierId == supplierId.Value);

        if (storeId.HasValue)
            q = q.Where(x => x.StoreId == storeId.Value);

        if (status.HasValue)
            q = q.Where(x => x.Status == status.Value);

        if (orderDateFrom.HasValue)
            q = q.Where(x => x.OrderDate >= orderDateFrom.Value);

        if (orderDateTo.HasValue)
            q = q.Where(x => x.OrderDate <= orderDateTo.Value);

        if (isActive.HasValue)
            q = q.Where(x => x.IsActive == isActive.Value);

        return q;
    }
}
