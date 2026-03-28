using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using RetailNexus.Api.Validators;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Validators;

public class CreateStoreTypeValidatorTests
{
    private readonly Mock<IStoreTypeRepository> _repoMock = new();
    private readonly CreateStoreTypeRequestValidator _validator;

    public CreateStoreTypeValidatorTests()
    {
        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreType?)null);

        _validator = new CreateStoreTypeRequestValidator(_repoMock.Object);
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new StoreTypesController.CreateStoreTypeRequest("01", "直営店");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task StoreTypeCd_WhenEmpty_ShouldFail(string? code)
    {
        var request = new StoreTypesController.CreateStoreTypeRequest(code!, "直営店");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeCd)
            .WithErrorMessage("店舗種別コードは必須です。");
    }

    [Fact]
    public async Task StoreTypeCd_WhenTooLong_ShouldFail()
    {
        var request = new StoreTypesController.CreateStoreTypeRequest("123", "直営店");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeCd)
            .WithErrorMessage("店舗種別コードは2文字以内で入力してください。");
    }

    [Fact]
    public async Task StoreTypeCd_WhenDuplicate_ShouldFail()
    {
        var existing = new StoreType("01", "既存種別", 1, true, Guid.NewGuid());
        _repoMock
            .Setup(r => r.GetByCodeAsync("01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new StoreTypesController.CreateStoreTypeRequest("01", "直営店");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeCd)
            .WithErrorMessage("この店舗種別コードは既に使用されています。");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task StoreTypeName_WhenEmpty_ShouldFail(string? name)
    {
        var request = new StoreTypesController.CreateStoreTypeRequest("01", name!);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeName)
            .WithErrorMessage("店舗種別名は必須です。");
    }

    [Fact]
    public async Task StoreTypeName_WhenTooLong_ShouldFail()
    {
        var request = new StoreTypesController.CreateStoreTypeRequest("01", new string('あ', 21));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeName)
            .WithErrorMessage("店舗種別名は20文字以内で入力してください。");
    }
}

public class ReorderStoreTypesValidatorTests
{
    private readonly Mock<IStoreTypeRepository> _repoMock = new();
    private readonly ReorderStoreTypesRequestValidator _validator;

    public ReorderStoreTypesValidatorTests()
    {
        _validator = new ReorderStoreTypesRequestValidator(_repoMock.Object);
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _repoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids.Select(id =>
            {
                var st = new StoreType("01", "テスト", 1, true, Guid.NewGuid());
                typeof(StoreType).GetProperty("StoreTypeId")!.SetValue(st, id);
                return st;
            }).ToList());

        var request = new StoreTypesController.ReorderStoreTypesRequest(ids);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task WhenEmpty_ShouldFail()
    {
        var request = new StoreTypesController.ReorderStoreTypesRequest(new List<Guid>());

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeIds)
            .WithErrorMessage("店舗種別IDリストは必須です。");
    }

    [Fact]
    public async Task WhenDuplicateIds_ShouldFail()
    {
        var id = Guid.NewGuid();
        var request = new StoreTypesController.ReorderStoreTypesRequest(new List<Guid> { id, id });

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeIds)
            .WithErrorMessage("店舗種別IDリストに重複があります。");
    }
}
