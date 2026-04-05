using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IStoreTypeService
{
    Task<StoreType> CreateAsync(string code, string name, bool isActive, Guid actorId, CancellationToken ct);
    Task<StoreType> UpdateAsync(Guid id, string code, string name, Guid actorId, CancellationToken ct);
    Task<StoreType> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
    Task<IReadOnlyList<StoreType>> ReorderAsync(IReadOnlyList<Guid> orderedIds, Guid actorId, CancellationToken ct);
}
