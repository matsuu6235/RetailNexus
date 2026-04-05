using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class StoreTypeServiceTests
{
    private readonly Mock<IStoreTypeRepository> _repoMock = new();
    private readonly StoreTypeService _service;

    public StoreTypeServiceTests()
    {
        _service = new StoreTypeService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGetNextDisplayOrder_AndCreateEntity()
    {
        var actorId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetNextDisplayOrderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync("01", "直営店", true, actorId, CancellationToken.None);

        result.StoreTypeCd.Should().Be("01");
        result.StoreTypeName.Should().Be("直営店");
        result.DisplayOrder.Should().Be(1);
        result.IsActive.Should().BeTrue();

        _repoMock.Verify(r => r.AddAsync(It.IsAny<StoreType>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallEntityUpdate()
    {
        var actorId = Guid.NewGuid();
        var entity = new StoreType("01", "直営店", 1, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.StoreTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.UpdateAsync(entity.StoreTypeId, "02", "フランチャイズ", actorId, CancellationToken.None);

        result.StoreTypeCd.Should().Be("02");
        result.StoreTypeName.Should().Be("フランチャイズ");
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreType?)null);

        var act = () => _service.UpdateAsync(id, "02", "フランチャイズ", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var entity = new StoreType("01", "直営店", 1, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.StoreTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.ChangeActivationAsync(entity.StoreTypeId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReorderAsync_ShouldSetDisplayOrders()
    {
        var actorId = Guid.NewGuid();
        var entity1 = new StoreType("01", "直営店", 1, true, actorId);
        var entity2 = new StoreType("02", "フランチャイズ", 2, true, actorId);

        var orderedIds = new List<Guid> { entity2.StoreTypeId, entity1.StoreTypeId };

        _repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoreType> { entity1, entity2 });

        var result = await _service.ReorderAsync(orderedIds, actorId, CancellationToken.None);

        entity2.DisplayOrder.Should().Be(1);
        entity1.DisplayOrder.Should().Be(2);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
