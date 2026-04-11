using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class PurchaseOrderMessageService : IPurchaseOrderMessageService
{
    private readonly IPurchaseOrderMessageRepository _messageRepo;
    private readonly IPurchaseOrderRepository _orderRepo;

    public PurchaseOrderMessageService(
        IPurchaseOrderMessageRepository messageRepo,
        IPurchaseOrderRepository orderRepo)
    {
        _messageRepo = messageRepo;
        _orderRepo = orderRepo;
    }

    public async Task<IReadOnlyList<PurchaseOrderMessage>> GetMessagesAsync(Guid purchaseOrderId, CancellationToken ct)
    {
        _ = await _orderRepo.GetByIdAsync(purchaseOrderId, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", purchaseOrderId);

        return await _messageRepo.ListByOrderIdAsync(purchaseOrderId, ct);
    }

    public async Task<PurchaseOrderMessage> SendMessageAsync(Guid purchaseOrderId, Guid sentBy, string body, CancellationToken ct)
    {
        _ = await _orderRepo.GetByIdAsync(purchaseOrderId, ct)
            ?? throw new EntityNotFoundException("PurchaseOrder", purchaseOrderId);

        var message = new PurchaseOrderMessage(purchaseOrderId, sentBy, body);
        await _messageRepo.AddAsync(message, ct);
        await _messageRepo.SaveChangesAsync(ct);

        var messages = await _messageRepo.ListByOrderIdAsync(purchaseOrderId, ct);
        return messages.First(m => m.PurchaseOrderMessageId == message.PurchaseOrderMessageId);
    }
}
