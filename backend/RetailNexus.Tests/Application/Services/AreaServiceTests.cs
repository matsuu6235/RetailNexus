using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class AreaServiceTests
{
    private readonly Mock<IAreaRepository> _repoMock = new();
    private readonly AreaService _service;

    public AreaServiceTests()
    {
        _service = new AreaService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGetNextDisplayOrder_AndCreateEntity()
    {
        var actorId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetNextDisplayOrderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync("01", "関東", true, actorId, CancellationToken.None);

        result.AreaCode.Should().Be("01");
        result.AreaName.Should().Be("関東");
        result.DisplayOrder.Should().Be(1);
        result.IsActive.Should().BeTrue();

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Area>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallEntityUpdate()
    {
        var actorId = Guid.NewGuid();
        var entity = new Area("01", "関東", 1, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.AreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.UpdateAsync(entity.AreaId, "02", "関西", actorId, CancellationToken.None);

        result.AreaCode.Should().Be("02");
        result.AreaName.Should().Be("関西");
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Area?)null);

        var act = () => _service.UpdateAsync(id, "02", "関西", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var entity = new Area("01", "関東", 1, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.AreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.ChangeActivationAsync(entity.AreaId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReorderAsync_ShouldSetDisplayOrders()
    {
        var actorId = Guid.NewGuid();
        var entity1 = new Area("01", "関東", 1, true, actorId);
        var entity2 = new Area("02", "関西", 2, true, actorId);

        var orderedIds = new List<Guid> { entity2.AreaId, entity1.AreaId };

        _repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Area> { entity1, entity2 });

        var result = await _service.ReorderAsync(orderedIds, actorId, CancellationToken.None);

        entity2.DisplayOrder.Should().Be(1);
        entity1.DisplayOrder.Should().Be(2);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
