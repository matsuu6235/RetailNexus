using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly RetailNexusDbContext _db;

    public AuditLogRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public async Task<List<AuditLog>> ListAsync(
        DateTimeOffset? from, DateTimeOffset? to,
        string? userName, string? action, string? entityName,
        int skip, int take, CancellationToken ct)
    {
        var query = BuildQuery(from, to, userName, action, entityName);
        return await query
            .OrderByDescending(x => x.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        DateTimeOffset? from, DateTimeOffset? to,
        string? userName, string? action, string? entityName,
        CancellationToken ct)
    {
        var query = BuildQuery(from, to, userName, action, entityName);
        return await query.CountAsync(ct);
    }

    private IQueryable<AuditLog> BuildQuery(
        DateTimeOffset? from, DateTimeOffset? to,
        string? userName, string? action, string? entityName)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (from.HasValue)
            query = query.Where(x => x.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.Timestamp <= to.Value);
        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(x => x.UserName.Contains(userName));
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);
        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(x => x.EntityName == entityName);

        return query;
    }
}
