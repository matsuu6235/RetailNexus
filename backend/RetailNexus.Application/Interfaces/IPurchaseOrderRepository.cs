using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PurchaseOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrder>> ListAsync(
        string? orderNumber,
        Guid? supplierId,
        Guid? storeId,
        PurchaseOrderStatus? status,
        DateTimeOffset? orderDateFrom,
        DateTimeOffset? orderDateTo,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct);
    Task<int> CountAsync(
        string? orderNumber,
        Guid? supplierId,
        Guid? storeId,
        PurchaseOrderStatus? status,
        DateTimeOffset? orderDateFrom,
        DateTimeOffset? orderDateTo,
        bool? isActive,
        CancellationToken ct);
    Task<string?> GetMaxOrderNumberAsync(CancellationToken ct);
    Task AddAsync(PurchaseOrder order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    void RemoveDetails(IEnumerable<PurchaseOrderDetail> details);
    void AddDetail(PurchaseOrderDetail detail);
}
