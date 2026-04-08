using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class AreaService : IAreaService
{
    private readonly IAreaRepository _repo;

    public AreaService(IAreaRepository repo)
    {
        _repo = repo;
    }

    public async Task<Area> CreateAsync(string areaCd, string areaName, bool isActive, Guid actorId, CancellationToken ct)
    {
        var code = areaCd.Trim();
        var name = areaName.Trim();
        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);

        var entity = new Area(code, name, nextDisplayOrder, isActive, actorId);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<Area> UpdateAsync(Guid id, string areaCd, string areaName, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Area", id);

        var trimmedCode = areaCd.Trim();
        var trimmedName = areaName.Trim();
        entity.Update(trimmedCode, trimmedName, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<Area> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Area", id);

        entity.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<IReadOnlyList<Area>> ReorderAsync(IReadOnlyList<Guid> orderedIds, Guid actorId, CancellationToken ct)
    {
        var distinctIds = orderedIds.Distinct().ToArray();
        var entities = await _repo.GetByIdsAsync(distinctIds, ct);

        var orderMap = orderedIds
            .Select((id, index) => new { id, displayOrder = index + 1 })
            .ToDictionary(x => x.id, x => x.displayOrder);

        foreach (var entity in entities)
        {
            entity.SetDisplayOrder(orderMap[entity.AreaId], actorId);
        }

        await _repo.SaveChangesAsync(ct);

        return entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.AreaCd)
            .ToList();
    }
}
