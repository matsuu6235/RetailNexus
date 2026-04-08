using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly RetailNexusDbContext _db;

    public SupplierRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<Supplier?> GetByIdAsync(Guid supplierId, CancellationToken ct)
        => _db.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == supplierId, ct);

    public Task<Supplier?> GetBySupplierCodeAsync(string supplierCode, CancellationToken ct)
        => _db.Suppliers.FirstOrDefaultAsync(x => x.SupplierCode == supplierCode, ct);

    public async Task<IReadOnlyList<Supplier>> ListAsync(
        string? supplierCode,
        string? supplierName,
        string? phoneNumber,
        string? email,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct)
    {
        var q = BuildQuery(supplierCode, supplierName, phoneNumber, email, isActive);

        return await q
            .OrderBy(x => x.SupplierCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? supplierCode,
        string? supplierName,
        string? phoneNumber,
        string? email,
        bool? isActive,
        CancellationToken ct)
    {
        var q = BuildQuery(supplierCode, supplierName, phoneNumber, email, isActive);
        return await q.CountAsync(ct);
    }

    public async Task<string?> GetMaxSupplierCodeAsync(CancellationToken ct)
    {
        return await _db.Suppliers
            .OrderByDescending(x => x.SupplierCode)
            .Select(x => x.SupplierCode)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Supplier supplier, CancellationToken ct)
        => await _db.Suppliers.AddAsync(supplier, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private IQueryable<Supplier> BuildQuery(
        string? supplierCode,
        string? supplierName,
        string? phoneNumber,
        string? email,
        bool? isActive)
    {
        var q = _db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(supplierCode))
            q = q.Where(x => x.SupplierCode.Contains(supplierCode));

        if (!string.IsNullOrWhiteSpace(supplierName))
            q = q.Where(x => x.SupplierName.Contains(supplierName));

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            q = q.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(phoneNumber));

        if (!string.IsNullOrWhiteSpace(email))
            q = q.Where(x => x.Email != null && x.Email.Contains(email));

        if (isActive.HasValue)
        {
            q = q.Where(x => x.IsActive == isActive.Value);
        }

        return q;
    }
}
