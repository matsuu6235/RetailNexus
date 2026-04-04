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

public class CreateSupplierValidatorTests
{
    private readonly Mock<ISupplierRepository> _repoMock = new();
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly CreateSupplierRequestValidator _validator;

    public CreateSupplierValidatorTests()
    {
        _validator = new CreateSupplierRequestValidator(_repoMock.Object, _localizer);
    }

    private static SuppliersController.CreateSupplierRequest ValidRequest()
        => new("テスト仕入先", "03-1234-5678", "test@example.com");

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidRequest_WithoutOptionalFields_ShouldPass()
    {
        var request = new SuppliersController.CreateSupplierRequest("テスト仕入先", null, null);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SupplierName_WhenEmpty_ShouldFail(string? name)
    {
        var request = new SuppliersController.CreateSupplierRequest(name!, null, null);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.SupplierName)
            .WithErrorMessage("仕入先名は必須です。");
    }

    [Fact]
    public async Task SupplierName_WhenTooLong_ShouldFail()
    {
        var request = new SuppliersController.CreateSupplierRequest(new string('あ', 51), null, null);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.SupplierName)
            .WithErrorMessage("仕入先名は50文字以内で入力してください。");
    }

    [Fact]
    public async Task PhoneNumber_WhenTooLong_ShouldFail()
    {
        var request = new SuppliersController.CreateSupplierRequest("テスト仕入先", new string('0', 21), null);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("電話番号は20文字以内で入力してください。");
    }

    [Fact]
    public async Task Email_WhenInvalidFormat_ShouldFail()
    {
        var request = new SuppliersController.CreateSupplierRequest("テスト仕入先", null, "invalid-email");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("メールアドレスの形式が正しくありません。");
    }

    [Fact]
    public async Task Email_WhenTooLong_ShouldFail()
    {
        var request = new SuppliersController.CreateSupplierRequest("テスト仕入先", null, new string('a', 250) + "@b.com");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("メールアドレスは255文字以内で入力してください。");
    }

    [Fact]
    public async Task Email_WhenEmptyString_ShouldNotValidate()
    {
        // .When(x => !string.IsNullOrEmpty(x.Email)) により、空文字はバリデーション対象外
        var request = new SuppliersController.CreateSupplierRequest("テスト仕入先", null, "");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
