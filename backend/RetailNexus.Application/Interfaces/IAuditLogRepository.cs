using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> ListAsync(
        DateTimeOffset? from, DateTimeOffset? to,
        string? userName, string? action, string? entityName,
        int skip, int take, CancellationToken ct);
    Task<int> CountAsync(
        DateTimeOffset? from, DateTimeOffset? to,
        string? userName, string? action, string? entityName,
        CancellationToken ct);
}
