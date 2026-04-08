using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<Supplier> CreateAsync(string supplierName, string? phoneNumber, string? supplierEmail, bool isActive, Guid actorId, CancellationToken ct);
    Task<Supplier> UpdateAsync(Guid id, string supplierName, string? phoneNumber, string? supplierEmail, Guid actorId, CancellationToken ct);
    Task<Supplier> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
