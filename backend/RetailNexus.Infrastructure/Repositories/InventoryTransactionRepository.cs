using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class InventoryTransactionRepository : IInventoryTransactionRepository
{
    private readonly RetailNexusDbContext _db;

    public InventoryTransactionRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<InventoryTransaction>> ListAsync(
        Guid? storeId,
        Guid? productId,
        InventoryTransactionType? transactionType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        int skip,
        int take,
        CancellationToken ct)
    {
        return await BuildQuery(storeId, productId, transactionType, dateFrom, dateTo)
            .Include(x => x.Store)
            .Include(x => x.Product)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        Guid? storeId,
        Guid? productId,
        InventoryTransactionType? transactionType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken ct)
    {
        return BuildQuery(storeId, productId, transactionType, dateFrom, dateTo)
            .CountAsync(ct);
    }

    public async Task AddAsync(InventoryTransaction transaction, CancellationToken ct)
        => await _db.InventoryTransactions.AddAsync(transaction, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private IQueryable<InventoryTransaction> BuildQuery(
        Guid? storeId,
        Guid? productId,
        InventoryTransactionType? transactionType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo)
    {
        var q = _db.InventoryTransactions.AsQueryable();

        if (storeId.HasValue)
            q = q.Where(x => x.StoreId == storeId.Value);

        if (productId.HasValue)
            q = q.Where(x => x.ProductId == productId.Value);

        if (transactionType.HasValue)
            q = q.Where(x => x.TransactionType == transactionType.Value);

        if (dateFrom.HasValue)
            q = q.Where(x => x.OccurredAt >= dateFrom.Value);

        if (dateTo.HasValue)
            q = q.Where(x => x.OccurredAt <= dateTo.Value);

        return q;
    }
}
