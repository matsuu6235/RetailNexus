using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IPurchaseOrderMessageRepository
{
    Task<IReadOnlyList<PurchaseOrderMessage>> ListByOrderIdAsync(Guid purchaseOrderId, CancellationToken ct);
    Task AddAsync(PurchaseOrderMessage message, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
