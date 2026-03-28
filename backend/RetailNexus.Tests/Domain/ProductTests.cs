using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var product = new Product("FD-000001", "4901234567890", "テスト商品", 1000m, 500m, "CAT01");

        product.ProductCode.Should().Be("FD-000001");
        product.JanCode.Should().Be("4901234567890");
        product.ProductName.Should().Be("テスト商品");
        product.Price.Should().Be(1000m);
        product.Cost.Should().Be(500m);
        product.ProductCategoryCode.Should().Be("CAT01");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        var product = new Product("FD-000001", "", "テスト商品", 0, 0, "CAT01");

        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_ShouldAllowEmptyJanCode()
    {
        var product = new Product("FD-000001", "", "テスト商品", 100m, 50m, "CAT01");

        product.JanCode.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldModifyAllProperties()
    {
        var product = new Product("FD-000001", "4901234567890", "テスト商品", 1000m, 500m, "CAT01");

        product.Update("4901234567891", "更新商品", 2000m, 800m, "CAT02");

        product.ProductCode.Should().Be("FD-000001");
        product.JanCode.Should().Be("4901234567891");
        product.ProductName.Should().Be("更新商品");
        product.Price.Should().Be(2000m);
        product.Cost.Should().Be(800m);
        product.ProductCategoryCode.Should().Be("CAT02");
    }

    [Fact]
    public void SetActivation_ShouldChangeIsActiveAndUpdateTimestamp()
    {
        var product = new Product("FD-000001", "", "テスト商品", 100m, 50m, "CAT01");
        var before = product.UpdatedAt;

        product.SetActivation(false);

        product.IsActive.Should().BeFalse();
        product.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldUpdateTimestamp()
    {
        var product = new Product("FD-000001", "", "テスト商品", 100m, 50m, "CAT01");
        var before = product.UpdatedAt;

        product.Update("", "テスト商品改", 200m, 100m, "CAT01");

        product.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldAllowZeroPrice()
    {
        var product = new Product("FD-000001", "", "テスト商品", 100m, 50m, "CAT01");

        product.Update("", "テスト商品", 0m, 0m, "CAT01");

        product.Price.Should().Be(0m);
        product.Cost.Should().Be(0m);
    }

    [Fact]
    public void Update_ShouldNotChangeProductCode()
    {
        var product = new Product("FD-000001", "", "テスト商品", 100m, 50m, "CAT01");

        product.Update("", "更新商品", 200m, 100m, "CAT02");

        product.ProductCode.Should().Be("FD-000001");
    }
}
