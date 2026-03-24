using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken ct);
    Task<ProductCategory?> GetByAbbreviationAsync(string abbreviation, CancellationToken ct);
    Task<ProductCategory?> GetByAbbreviationExcludingAsync(string abbreviation, Guid excludeId, CancellationToken ct);
    Task<int> CountAsync(string? code, string? name, bool? isActive, CancellationToken ct);
    Task<IReadOnlyList<ProductCategory>> ListAsync(string? code, string? name, bool? isActive, int skip, int take, CancellationToken ct);
    Task<IReadOnlyList<ProductCategory>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    Task<int> GetNextDisplayOrderAsync(CancellationToken ct);
    Task AddAsync(ProductCategory entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}