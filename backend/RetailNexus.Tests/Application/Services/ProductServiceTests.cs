using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IProductCategoryRepository> _categoryRepoMock = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_productRepoMock.Object, _categoryRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldLookupCategory_GenerateCode_CreateProduct()
    {
        var actorId = Guid.NewGuid();
        var category = new ProductCategory("01", "FD", "食品", 1, true, actorId);

        _categoryRepoMock.Setup(r => r.GetByCodeAsync("01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock.Setup(r => r.GetMaxProductCodeByPrefixAsync("FD", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _service.CreateAsync("4901234567890", "テスト商品", 100m, 80m, "01", actorId, CancellationToken.None);

        result.ProductCode.Should().Be("FD-000001");
        result.JanCode.Should().Be("4901234567890");
        result.ProductName.Should().Be("テスト商品");
        result.Price.Should().Be(100m);
        result.Cost.Should().Be(80m);

        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldThrowEntityNotFoundException()
    {
        _categoryRepoMock.Setup(r => r.GetByCodeAsync("99", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        var act = () => _service.CreateAsync("4901234567890", "テスト商品", 100m, 80m, "99", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallProductUpdate()
    {
        var actorId = Guid.NewGuid();
        var product = new Product("FD-000001", "4901234567890", "テスト商品", 100m, 80m, "01", actorId);

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.UpdateAsync(product.Id, "4901234567891", "更新商品", 200m, 150m, "02", actorId, CancellationToken.None);

        result.JanCode.Should().Be("4901234567891");
        result.ProductName.Should().Be("更新商品");
        result.Price.Should().Be(200m);
        result.Cost.Should().Be(150m);
        _productRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = () => _service.UpdateAsync(id, "4901234567890", "テスト商品", 100m, 80m, "01", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var product = new Product("FD-000001", "4901234567890", "テスト商品", 100m, 80m, "01", actorId);

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.ChangeActivationAsync(product.Id, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _productRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
