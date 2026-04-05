using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IStoreService
{
    Task<Store> CreateAsync(string storeName, Guid areaId, Guid storeTypeId, bool isActive, Guid actorId, CancellationToken ct);
    Task<Store> UpdateAsync(Guid id, string storeName, Guid areaId, Guid storeTypeId, Guid actorId, CancellationToken ct);
    Task<Store> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
