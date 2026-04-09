using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class StoreTypeService : IStoreTypeService
{
    private readonly IStoreTypeRepository _repo;

    public StoreTypeService(IStoreTypeRepository repo)
    {
        _repo = repo;
    }

    public async Task<StoreType> CreateAsync(string storeTypeCode, string storeTypeName, bool isActive, Guid actorId, CancellationToken ct)
    {
        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);

        var trimmedStoreTypeCode = storeTypeCode.Trim();
        var trimmedStoreTypeName = storeTypeName.Trim();
        var entity = new StoreType(trimmedStoreTypeCode, trimmedStoreTypeName, nextDisplayOrder, isActive, actorId);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<StoreType> UpdateAsync(Guid id, string storeTypeCode, string storeTypeName, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreType", id);

        var trimmedStoreTypeCode = storeTypeCode.Trim();
        var trimmedStoreTypeName = storeTypeName.Trim();
        entity.Update(trimmedStoreTypeCode, trimmedStoreTypeName, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<StoreType> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("StoreType", id);

        entity.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<IReadOnlyList<StoreType>> ReorderAsync(IReadOnlyList<Guid> orderedIds, Guid actorId, CancellationToken ct)
    {
        var distinctIds = orderedIds.Distinct().ToArray();
        var entities = await _repo.GetByIdsAsync(distinctIds, ct);

        var orderMap = orderedIds
            .Select((id, index) => new { id, displayOrder = index + 1 })
            .ToDictionary(x => x.id, x => x.displayOrder);

        foreach (var entity in entities)
        {
            entity.SetDisplayOrder(orderMap[entity.StoreTypeId], actorId);
        }

        await _repo.SaveChangesAsync(ct);

        return entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.StoreTypeCode)
            .ToList();
    }
}
