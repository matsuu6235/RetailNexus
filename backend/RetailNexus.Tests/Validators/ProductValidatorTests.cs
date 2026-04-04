using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Moq;
using RetailNexus.Api.Controllers;
using RetailNexus.Api.Validators;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Resources;
using RetailNexus.Tests.Helpers;

namespace RetailNexus.Tests.Validators;

public class CreateProductValidatorTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IProductCategoryRepository> _categoryRepoMock = new();
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly CreateProductRequestValidator _validator;

    public CreateProductValidatorTests()
    {
        _productRepoMock
            .Setup(r => r.GetByProductCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var activeCategory = new ProductCategory("CAT01", "FD", "カテゴリ", 1, true, Guid.NewGuid());
        _categoryRepoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCategory);

        _validator = new CreateProductRequestValidator(_productRepoMock.Object, _categoryRepoMock.Object, _localizer);
    }

    private static ProductsController.CreateProductRequest ValidRequest()
        => new("4901234567890", "テスト商品", 1000m, 500m, "CAT01");

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidRequest_WithEmptyJanCode_ShouldPass()
    {
        var request = new ProductsController.CreateProductRequest("", "テスト商品", 1000m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.JanCode);
    }

    [Fact]
    public async Task JanCode_WhenNonNumeric_ShouldFail()
    {
        var request = new ProductsController.CreateProductRequest("490123456789A", "テスト商品", 1000m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.JanCode)
            .WithErrorMessage("JANコードは数字のみ入力できます。");
    }

    [Fact]
    public async Task JanCode_WhenNot13Digits_ShouldFail()
    {
        var request = new ProductsController.CreateProductRequest("123456789012", "テスト商品", 1000m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.JanCode)
            .WithErrorMessage("JANコードは13桁で入力してください。");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ProductName_WhenEmpty_ShouldFail(string? name)
    {
        var request = new ProductsController.CreateProductRequest("", name!, 1000m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductName)
            .WithErrorMessage("商品名は必須です。");
    }

    [Fact]
    public async Task ProductName_WhenTooLong_ShouldFail()
    {
        var request = new ProductsController.CreateProductRequest("", new string('あ', 201), 1000m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductName)
            .WithErrorMessage("商品名は200文字以内で入力してください。");
    }

    [Fact]
    public async Task ProductCategoryCode_WhenCategoryInactive_ShouldFail()
    {
        var inactiveCategory = new ProductCategory("CAT01", "FD", "無効カテゴリ", 1, false, Guid.NewGuid());
        _categoryRepoMock
            .Setup(r => r.GetByCodeAsync("CAT01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveCategory);

        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryCode)
            .WithErrorMessage("指定された商品カテゴリが存在しないか、無効になっています。");
    }

    [Fact]
    public async Task ProductCategoryCode_WhenCategoryNotFound_ShouldFail()
    {
        _categoryRepoMock
            .Setup(r => r.GetByCodeAsync("NOTEXIST", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        var request = new ProductsController.CreateProductRequest("", "テスト商品", 1000m, 500m, "NOTEXIST");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryCode);
    }

    [Fact]
    public async Task Price_WhenNegative_ShouldFail()
    {
        var request = new ProductsController.CreateProductRequest("", "テスト商品", -1m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("売価は0以上で入力してください。");
    }

    [Fact]
    public async Task Cost_WhenNegative_ShouldFail()
    {
        var request = new ProductsController.CreateProductRequest("", "テスト商品", 1000m, -1m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Cost)
            .WithErrorMessage("原価は0以上で入力してください。");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(99999.99)]
    public async Task Price_WhenZeroOrPositive_ShouldPass(decimal price)
    {
        var request = new ProductsController.CreateProductRequest("", "テスト商品", price, 0m, "CAT01");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }
}

public class UpdateProductValidatorTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IProductCategoryRepository> _categoryRepoMock = new();
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly UpdateProductRequestValidator _validator;
    private readonly Guid _productId = Guid.NewGuid();

    public UpdateProductValidatorTests()
    {
        _productRepoMock
            .Setup(r => r.GetByProductCodeExcludingAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var activeCategory = new ProductCategory("CAT01", "FD", "カテゴリ", 1, true, Guid.NewGuid());
        _categoryRepoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCategory);

        _validator = new UpdateProductRequestValidator(_productRepoMock.Object, _categoryRepoMock.Object, _localizer);
    }

    private ValidationContext<ProductsController.UpdateProductRequest> CreateContext(ProductsController.UpdateProductRequest request)
    {
        var ctx = new ValidationContext<ProductsController.UpdateProductRequest>(request);
        ctx.RootContextData["productId"] = _productId;
        return ctx;
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new ProductsController.UpdateProductRequest("4901234567890", "テスト商品", 1000m, 500m, "CAT01");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
