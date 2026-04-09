using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class StoreServiceTests
{
    private readonly Mock<IStoreRepository> _repoMock = new();
    private readonly StoreService _service;

    public StoreServiceTests()
    {
        _service = new StoreService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateCode_AndRetrieveWithNavigation()
    {
        var actorId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var storeTypeId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetMaxStoreCodeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // GetByIdAsync is called after creation to re-fetch with navigation properties
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Store("000001", "テスト店舗", areaId, storeTypeId, true, actorId));

        var result = await _service.CreateAsync("テスト店舗", areaId, storeTypeId, true, actorId, CancellationToken.None);

        result.StoreCode.Should().Be("000001");
        result.StoreName.Should().Be("テスト店舗");
        result.AreaId.Should().Be(areaId);
        result.StoreTypeId.Should().Be(storeTypeId);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallEntityUpdate_AndRetrieve()
    {
        var actorId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var newAreaId = Guid.NewGuid();
        var storeTypeId = Guid.NewGuid();
        var entity = new Store("000001", "店舗A", areaId, storeTypeId, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.UpdateAsync(entity.StoreId, "店舗B", newAreaId, storeTypeId, actorId, CancellationToken.None);

        result.StoreName.Should().Be("店舗B");
        result.AreaId.Should().Be(newAreaId);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // GetByIdAsync called twice: once for fetch, once for re-fetch after update
        _repoMock.Verify(r => r.GetByIdAsync(entity.StoreId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        var act = () => _service.UpdateAsync(id, "店舗B", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var entity = new Store("000001", "店舗A", Guid.NewGuid(), Guid.NewGuid(), true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.ChangeActivationAsync(entity.StoreId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
