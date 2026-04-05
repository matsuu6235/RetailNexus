using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IProductCategoryService
{
    Task<ProductCategory> CreateAsync(string code, string abbreviation, string name, bool isActive, Guid actorId, CancellationToken ct);
    Task<ProductCategory> UpdateAsync(Guid id, string code, string abbreviation, string name, Guid actorId, CancellationToken ct);
    Task<ProductCategory> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
    Task<IReadOnlyList<ProductCategory>> ReorderAsync(IReadOnlyList<Guid> orderedIds, Guid actorId, CancellationToken ct);
}
