using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Moq;
using RetailNexus.Api.Validators;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;
using RetailNexus.Domain.Entities;
using RetailNexus.Resources;
using RetailNexus.Tests.Helpers;

namespace RetailNexus.Tests.Validators;

public class CreateProductCategoryValidatorTests
{
    private readonly Mock<IProductCategoryRepository> _repoMock = new();
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly CreateProductCategoryRequestValidator _validator;

    public CreateProductCategoryValidatorTests()
    {
        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        _validator = new CreateProductCategoryRequestValidator(_repoMock.Object, _localizer);
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new ProductCategoriesController.CreateProductCategoryRequest("001", "FD", "食品");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ProductCategoryCd_WhenEmpty_ShouldFail(string? code)
    {
        var request = new ProductCategoriesController.CreateProductCategoryRequest(code!, "FD", "食品");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryCd)
            .WithErrorMessage("商品カテゴリコードは必須です。");
    }

    [Fact]
    public async Task ProductCategoryCd_WhenTooLong_ShouldFail()
    {
        var request = new ProductCategoriesController.CreateProductCategoryRequest(new string('0', 4), "FD", "食品");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryCd)
            .WithErrorMessage("商品カテゴリコードは3文字以内で入力してください。");
    }

    [Fact]
    public async Task ProductCategoryCd_WhenDuplicate_ShouldFail()
    {
        var existing = new ProductCategory("001", "FD", "既存カテゴリ", 1, true, Guid.NewGuid());
        _repoMock
            .Setup(r => r.GetByCodeAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new ProductCategoriesController.CreateProductCategoryRequest("001", "FD", "食品");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryCd)
            .WithErrorMessage("この商品カテゴリコードは既に使用されています。");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ProductCategoryName_WhenEmpty_ShouldFail(string? name)
    {
        var request = new ProductCategoriesController.CreateProductCategoryRequest("001", "FD", name!);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryName)
            .WithErrorMessage("商品カテゴリ名は必須です。");
    }

    [Fact]
    public async Task ProductCategoryName_WhenTooLong_ShouldFail()
    {
        var request = new ProductCategoriesController.CreateProductCategoryRequest("001", "FD", new string('あ', 31));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryName)
            .WithErrorMessage("商品カテゴリ名は30文字以内で入力してください。");
    }

    [Fact]
    public async Task ProductCategoryName_WhenExactlyMaxLength_ShouldPass()
    {
        var request = new ProductCategoriesController.CreateProductCategoryRequest("001", "FD", new string('あ', 30));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ProductCategoryName);
    }
}

public class UpdateProductCategoryValidatorTests
{
    private readonly Mock<IProductCategoryRepository> _repoMock = new();
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly UpdateProductCategoryRequestValidator _validator;
    private readonly Guid _entityId = Guid.NewGuid();

    public UpdateProductCategoryValidatorTests()
    {
        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        _validator = new UpdateProductCategoryRequestValidator(_repoMock.Object, _localizer);
    }

    private ValidationContext<ProductCategoriesController.UpdateProductCategoryRequest> CreateContext(
        ProductCategoriesController.UpdateProductCategoryRequest request)
    {
        var ctx = new ValidationContext<ProductCategoriesController.UpdateProductCategoryRequest>(request);
        ctx.RootContextData["EntityId"] = _entityId;
        return ctx;
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new ProductCategoriesController.UpdateProductCategoryRequest("001", "FD", "食品");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ProductCategoryCd_WhenSameEntity_ShouldPass()
    {
        var existing = new ProductCategory("001", "FD", "食品", 1, true, Guid.NewGuid());
        typeof(ProductCategory).GetProperty("ProductCategoryId")!.SetValue(existing, _entityId);

        _repoMock
            .Setup(r => r.GetByCodeAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new ProductCategoriesController.UpdateProductCategoryRequest("001", "FD", "食品更新");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldNotHaveValidationErrorFor(x => x.ProductCategoryCd);
    }

    [Fact]
    public async Task ProductCategoryCd_WhenDifferentEntity_ShouldFail()
    {
        var other = new ProductCategory("001", "FD", "他のカテゴリ", 1, true, Guid.NewGuid());

        _repoMock
            .Setup(r => r.GetByCodeAsync("001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(other);

        var request = new ProductCategoriesController.UpdateProductCategoryRequest("001", "FD", "食品");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryCd)
            .WithErrorMessage("この商品カテゴリコードは既に使用されています。");
    }
}

public class ReorderProductCategoriesValidatorTests
{
    private readonly Mock<IProductCategoryRepository> _repoMock = new();
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly ReorderProductCategoriesRequestValidator _validator;

    public ReorderProductCategoriesValidatorTests()
    {
        _validator = new ReorderProductCategoriesRequestValidator(_repoMock.Object, _localizer);
    }

    [Fact]
    public async Task WhenEmpty_ShouldFail()
    {
        var request = new ProductCategoriesController.ReorderProductCategoriesRequest(new List<Guid>());

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryIds)
            .WithErrorMessage("商品カテゴリIDリストは必須です。");
    }

    [Fact]
    public async Task WhenDuplicateIds_ShouldFail()
    {
        var id = Guid.NewGuid();
        var request = new ProductCategoriesController.ReorderProductCategoriesRequest(new List<Guid> { id, id });

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryIds)
            .WithErrorMessage("商品カテゴリIDリストに重複があります。");
    }

    [Fact]
    public async Task WhenNonExistentIds_ShouldFail()
    {
        var ids = new List<Guid> { Guid.NewGuid() };
        _repoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategory>());

        var request = new ProductCategoriesController.ReorderProductCategoriesRequest(ids);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategoryIds)
            .WithErrorMessage("存在しない商品カテゴリIDが含まれています。");
    }
}
