using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class StoreRequestRepository : IStoreRequestRepository
{
    private readonly RetailNexusDbContext _db;

    public StoreRequestRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<StoreRequest?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.StoreRequests
            .Include(x => x.FromStore)
            .Include(x => x.ToStore)
            .Include(x => x.Approver)
            .FirstOrDefaultAsync(x => x.StoreRequestId == id, ct);

    public Task<StoreRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct)
        => _db.StoreRequests
            .Include(x => x.FromStore)
            .Include(x => x.ToStore)
            .Include(x => x.Approver)
            .Include(x => x.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.StoreRequestId == id, ct);

    public async Task<IReadOnlyList<StoreRequest>> ListAsync(
        string? requestNumber,
        Guid? fromStoreId,
        Guid? toStoreId,
        StoreRequestStatus? status,
        DateTimeOffset? requestDateFrom,
        DateTimeOffset? requestDateTo,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct)
    {
        return await BuildQuery(requestNumber, fromStoreId, toStoreId, status, requestDateFrom, requestDateTo, isActive)
            .Include(x => x.FromStore)
            .Include(x => x.ToStore)
            .Include(x => x.Approver)
            .OrderByDescending(x => x.RequestDate)
            .ThenByDescending(x => x.RequestNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        string? requestNumber,
        Guid? fromStoreId,
        Guid? toStoreId,
        StoreRequestStatus? status,
        DateTimeOffset? requestDateFrom,
        DateTimeOffset? requestDateTo,
        bool? isActive,
        CancellationToken ct)
    {
        return BuildQuery(requestNumber, fromStoreId, toStoreId, status, requestDateFrom, requestDateTo, isActive)
            .CountAsync(ct);
    }

    public async Task<string?> GetMaxRequestNumberAsync(CancellationToken ct)
    {
        return await _db.StoreRequests
            .OrderByDescending(x => x.RequestNumber)
            .Select(x => x.RequestNumber)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(StoreRequest request, CancellationToken ct)
        => await _db.StoreRequests.AddAsync(request, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    public void RemoveDetails(IEnumerable<StoreRequestDetail> details)
        => _db.StoreRequestDetails.RemoveRange(details);

    public void AddDetail(StoreRequestDetail detail)
        => _db.StoreRequestDetails.Add(detail);

    private IQueryable<StoreRequest> BuildQuery(
        string? requestNumber,
        Guid? fromStoreId,
        Guid? toStoreId,
        StoreRequestStatus? status,
        DateTimeOffset? requestDateFrom,
        DateTimeOffset? requestDateTo,
        bool? isActive)
    {
        var q = _db.StoreRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(requestNumber))
        {
            var value = requestNumber.Trim();
            q = q.Where(x => x.RequestNumber.Contains(value));
        }

        if (fromStoreId.HasValue)
            q = q.Where(x => x.FromStoreId == fromStoreId.Value);

        if (toStoreId.HasValue)
            q = q.Where(x => x.ToStoreId == toStoreId.Value);

        if (status.HasValue)
            q = q.Where(x => x.Status == status.Value);

        if (requestDateFrom.HasValue)
            q = q.Where(x => x.RequestDate >= requestDateFrom.Value);

        if (requestDateTo.HasValue)
            q = q.Where(x => x.RequestDate <= requestDateTo.Value);

        if (isActive.HasValue)
            q = q.Where(x => x.IsActive == isActive.Value);

        return q;
    }
}
