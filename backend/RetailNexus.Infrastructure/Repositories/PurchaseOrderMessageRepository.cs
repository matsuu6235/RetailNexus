using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class PurchaseOrderMessageRepository : IPurchaseOrderMessageRepository
{
    private readonly RetailNexusDbContext _db;

    public PurchaseOrderMessageRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PurchaseOrderMessage>> ListByOrderIdAsync(Guid purchaseOrderId, CancellationToken ct)
        => await _db.PurchaseOrderMessages
            .Where(x => x.PurchaseOrderId == purchaseOrderId)
            .Include(x => x.Sender)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(PurchaseOrderMessage message, CancellationToken ct)
        => await _db.PurchaseOrderMessages.AddAsync(message, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
