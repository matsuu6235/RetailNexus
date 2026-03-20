using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Product?> GetByProductCodeAsync(string productCode, CancellationToken ct);

    Task<IReadOnlyList<Product>> ListAsync(
        string? productCode,
        string? janCode,
        string? productName,
        string? productCategoryCode,
        bool? isActive,
        int skip,
        int take,
        CancellationToken ct);

    Task<int> CountAsync(
        string? productCode,
        string? janCode,
        string? productName,
        string? productCategoryCode,
        bool? isActive,
        CancellationToken ct);

    Task AddAsync(Product product, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}