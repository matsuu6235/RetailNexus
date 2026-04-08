using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IAreaService
{
    Task<Area> CreateAsync(string areaCode, string areaName, bool isActive, Guid actorId, CancellationToken ct);
    Task<Area> UpdateAsync(Guid id, string areaCode, string areaName, Guid actorId, CancellationToken ct);
    Task<Area> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
    Task<IReadOnlyList<Area>> ReorderAsync(IReadOnlyList<Guid> orderedIds, Guid actorId, CancellationToken ct);
}
