using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<Supplier> CreateAsync(string name, string? phone, string? email, bool isActive, Guid actorId, CancellationToken ct);
    Task<Supplier> UpdateAsync(Guid id, string name, string? phone, string? email, Guid actorId, CancellationToken ct);
    Task<Supplier> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
