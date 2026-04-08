using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;

    public SupplierService(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public async Task<Supplier> CreateAsync(string supplierName, string? phoneNumber, string? supplierEmail, bool isActive, Guid actorId, CancellationToken ct)
    {
        var maxCode = await _repo.GetMaxSupplierCodeAsync(ct);
        var supplierCode = CodeGenerator.NextSupplierCode(maxCode);

        var trimmedSupplierName = supplierName.Trim();
        var supplier = new Supplier(supplierCode, trimmedSupplierName, phoneNumber, supplierEmail, isActive, actorId);
        await _repo.AddAsync(supplier, ct);
        await _repo.SaveChangesAsync(ct);

        return supplier;
    }

    public async Task<Supplier> UpdateAsync(Guid id, string supplierName, string? phoneNumber, string? supplierEmail, Guid actorId, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Supplier", id);

        var trimmedSupplierName = supplierName.Trim();
        supplier.Update(trimmedSupplierName, phoneNumber, supplierEmail, actorId);
        await _repo.SaveChangesAsync(ct);

        return supplier;
    }

    public async Task<Supplier> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Supplier", id);

        supplier.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return supplier;
    }
}
