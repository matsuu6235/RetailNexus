using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _repo;

    public ProductCategoryService(IProductCategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<ProductCategory> CreateAsync(string productCategoryCode, string categoryAbbreviation, string productCategoryName, bool isActive, Guid actorId, CancellationToken ct)
    {
        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);

        var trimmedProductCategoryCode = productCategoryCode.Trim();
        var trimmedCategoryAbbreviation = categoryAbbreviation.Trim();
        var trimmedProductCategoryName = productCategoryName.Trim();
        var entity = new ProductCategory(trimmedProductCategoryCode, trimmedCategoryAbbreviation, trimmedProductCategoryName, nextDisplayOrder, isActive, actorId);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<ProductCategory> UpdateAsync(Guid id, string productCategoryCode, string categoryAbbreviation, string productCategoryName, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("ProductCategory", id);

        var trimmedProductCategoryCode = productCategoryCode.Trim();
        var trimmedCategoryAbbreviation = categoryAbbreviation.Trim();
        var trimmedProductCategoryName = productCategoryName.Trim();
        entity.Update(trimmedProductCategoryCode, trimmedCategoryAbbreviation, trimmedProductCategoryName, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<ProductCategory> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("ProductCategory", id);

        entity.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<IReadOnlyList<ProductCategory>> ReorderAsync(IReadOnlyList<Guid> orderedIds, Guid actorId, CancellationToken ct)
    {
        var distinctIds = orderedIds.Distinct().ToArray();
        var entities = await _repo.GetByIdsAsync(distinctIds, ct);

        var orderMap = orderedIds
            .Select((id, index) => new { id, displayOrder = index + 1 })
            .ToDictionary(x => x.id, x => x.displayOrder);

        foreach (var entity in entities)
        {
            entity.SetDisplayOrder(orderMap[entity.ProductCategoryId], actorId);
        }

        await _repo.SaveChangesAsync(ct);

        return entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ProductCategoryCode)
            .ToList();
    }
}
