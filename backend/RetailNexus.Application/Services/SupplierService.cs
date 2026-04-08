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

    public async Task<Supplier> CreateAsync(string name, string? phone, string? email, bool isActive, Guid actorId, CancellationToken ct)
    {
        var maxCode = await _repo.GetMaxSupplierCodeAsync(ct);
        var supplierCode = CodeGenerator.NextSupplierCode(maxCode);

        var trimmedName = name.Trim();
        var supplier = new Supplier(supplierCode, trimmedName, phone, email, isActive, actorId);
        await _repo.AddAsync(supplier, ct);
        await _repo.SaveChangesAsync(ct);

        return supplier;
    }

    public async Task<Supplier> UpdateAsync(Guid id, string name, string? phone, string? email, Guid actorId, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Supplier", id);

        var trimmedName = name.Trim();
        supplier.Update(trimmedName, phone, email, actorId);
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
