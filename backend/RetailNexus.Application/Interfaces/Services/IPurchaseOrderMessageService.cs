using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IPurchaseOrderMessageService
{
    Task<IReadOnlyList<PurchaseOrderMessage>> GetMessagesAsync(Guid purchaseOrderId, CancellationToken ct);
    Task<PurchaseOrderMessage> SendMessageAsync(Guid purchaseOrderId, Guid sentBy, string body, CancellationToken ct);
}
