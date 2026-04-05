using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class ProductCategoryServiceTests
{
    private readonly Mock<IProductCategoryRepository> _repoMock = new();
    private readonly ProductCategoryService _service;

    public ProductCategoryServiceTests()
    {
        _service = new ProductCategoryService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGetNextDisplayOrder_AndCreateEntity()
    {
        var actorId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetNextDisplayOrderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync("01", "FD", "食品", true, actorId, CancellationToken.None);

        result.ProductCategoryCd.Should().Be("01");
        result.CategoryAbbreviation.Should().Be("FD");
        result.ProductCategoryName.Should().Be("食品");
        result.DisplayOrder.Should().Be(1);
        result.IsActive.Should().BeTrue();

        _repoMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallEntityUpdate()
    {
        var actorId = Guid.NewGuid();
        var entity = new ProductCategory("01", "FD", "食品", 1, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.ProductCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.UpdateAsync(entity.ProductCategoryId, "02", "BV", "飲料", actorId, CancellationToken.None);

        result.ProductCategoryCd.Should().Be("02");
        result.CategoryAbbreviation.Should().Be("BV");
        result.ProductCategoryName.Should().Be("飲料");
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        var act = () => _service.UpdateAsync(id, "02", "BV", "飲料", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var entity = new ProductCategory("01", "FD", "食品", 1, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.ProductCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.ChangeActivationAsync(entity.ProductCategoryId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReorderAsync_ShouldSetDisplayOrders()
    {
        var actorId = Guid.NewGuid();
        var entity1 = new ProductCategory("01", "FD", "食品", 1, true, actorId);
        var entity2 = new ProductCategory("02", "BV", "飲料", 2, true, actorId);

        var orderedIds = new List<Guid> { entity2.ProductCategoryId, entity1.ProductCategoryId };

        _repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategory> { entity1, entity2 });

        var result = await _service.ReorderAsync(orderedIds, actorId, CancellationToken.None);

        entity2.DisplayOrder.Should().Be(1);
        entity1.DisplayOrder.Should().Be(2);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
