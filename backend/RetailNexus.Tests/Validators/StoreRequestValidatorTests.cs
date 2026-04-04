using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Controllers;
using RetailNexus.Api.Validators;
using RetailNexus.Resources;
using RetailNexus.Tests.Helpers;

namespace RetailNexus.Tests.Validators;

public class CreateStoreRequestValidatorTests
{
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly CreateStoreRequestRequestValidator _validator;

    public CreateStoreRequestValidatorTests()
    {
        _validator = new CreateStoreRequestRequestValidator(_localizer);
    }

    private static StoreRequestsController.CreateStoreRequestRequest ValidRequest()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            null,
            new List<StoreRequestsController.CreateDetailRequest>
            {
                new(Guid.NewGuid(), 10),
            });

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task FromStoreId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with { FromStoreId = Guid.Empty };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FromStoreId)
            .WithErrorMessage("依頼元は必須です。");
    }

    [Fact]
    public async Task ToStoreId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with { ToStoreId = Guid.Empty };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ToStoreId)
            .WithErrorMessage("依頼先は必須です。");
    }

    [Fact]
    public async Task FromStoreId_And_ToStoreId_WhenSame_ShouldFail()
    {
        var storeId = Guid.NewGuid();
        var request = ValidRequest() with { FromStoreId = storeId, ToStoreId = storeId };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "依頼元と依頼先は異なる店舗を選択してください。");
    }

    [Fact]
    public async Task Note_WhenTooLong_ShouldFail()
    {
        var request = ValidRequest() with { Note = new string('あ', 501) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Note)
            .WithErrorMessage("備考は500文字以内で入力してください。");
    }

    [Fact]
    public async Task Details_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with { Details = new List<StoreRequestsController.CreateDetailRequest>() };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Details)
            .WithErrorMessage("明細を1行以上入力してください。");
    }

    [Fact]
    public async Task Details_WhenDuplicateProducts_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var request = ValidRequest() with
        {
            Details = new List<StoreRequestsController.CreateDetailRequest>
            {
                new(productId, 10),
                new(productId, 5),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Details)
            .WithErrorMessage("同一商品が複数行に含まれています。数量を変更してください。");
    }

    [Fact]
    public async Task Detail_Quantity_WhenZero_ShouldFail()
    {
        var request = ValidRequest() with
        {
            Details = new List<StoreRequestsController.CreateDetailRequest>
            {
                new(Guid.NewGuid(), 0),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e => e.ErrorMessage == "数量は1以上で入力してください。");
    }

    [Fact]
    public async Task Detail_ProductId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with
        {
            Details = new List<StoreRequestsController.CreateDetailRequest>
            {
                new(Guid.Empty, 10),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e => e.ErrorMessage == "商品は必須です。");
    }
}

public class UpdateStoreRequestValidatorTests
{
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly UpdateStoreRequestRequestValidator _validator;

    public UpdateStoreRequestValidatorTests()
    {
        _validator = new UpdateStoreRequestRequestValidator(_localizer);
    }

    private static StoreRequestsController.UpdateStoreRequestRequest ValidRequest()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            new List<StoreRequestsController.UpdateDetailRequest>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 10),
            });

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task FromStoreId_And_ToStoreId_WhenSame_ShouldFail()
    {
        var storeId = Guid.NewGuid();
        var request = ValidRequest() with { FromStoreId = storeId, ToStoreId = storeId };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "依頼元と依頼先は異なる店舗を選択してください。");
    }

    [Fact]
    public async Task Details_WhenDuplicateProducts_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var request = ValidRequest() with
        {
            Details = new List<StoreRequestsController.UpdateDetailRequest>
            {
                new(Guid.NewGuid(), productId, 10),
                new(null, productId, 5),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Details)
            .WithErrorMessage("同一商品が複数行に含まれています。数量を変更してください。");
    }
}
