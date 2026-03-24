using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid supplierId, CancellationToken ct);
    Task<Supplier?> GetBySupplierCodeAsync(string supplierCode, CancellationToken ct);
    Task<IReadOnlyList<Supplier>> ListAsync(
        string? supplierCode,
        string? supplierName,
        string? phoneNumber,
        string? email,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct);
    Task<int> CountAsync(
        string? supplierCode,
        string? supplierName,
        string? phoneNumber,
        string? email,
        bool? isActive,
        CancellationToken ct);
    Task<string?> GetMaxSupplierCodeAsync(CancellationToken ct);
    Task AddAsync(Supplier supplier, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
