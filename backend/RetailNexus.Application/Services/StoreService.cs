using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _repo;

    public StoreService(IStoreRepository repo)
    {
        _repo = repo;
    }

    public async Task<Store> CreateAsync(string storeName, Guid areaId, Guid storeTypeId, bool isActive, Guid actorId, CancellationToken ct)
    {
        var maxCode = await _repo.GetMaxStoreCodeAsync(ct);
        var storeCode = CodeGenerator.NextStoreCode(maxCode);

        var trimmedStoreName = storeName.Trim();
        var entity = new Store(storeCode, trimmedStoreName, areaId, storeTypeId, isActive, actorId);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        // ナビゲーションプロパティ（Area, StoreType）を含めて再取得
        var created = await _repo.GetByIdAsync(entity.StoreId, ct);
        return created!;
    }

    public async Task<Store> UpdateAsync(Guid id, string storeName, Guid areaId, Guid storeTypeId, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Store", id);

        var trimmedStoreName = storeName.Trim();
        entity.Update(trimmedStoreName, areaId, storeTypeId, actorId);
        await _repo.SaveChangesAsync(ct);

        // ナビゲーションプロパティを含めて再取得
        var updated = await _repo.GetByIdAsync(entity.StoreId, ct);
        return updated!;
    }

    public async Task<Store> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Store", id);

        entity.SetActivation(isActive, actorId);
        await _repo.SaveChangesAsync(ct);

        return entity;
    }
}
