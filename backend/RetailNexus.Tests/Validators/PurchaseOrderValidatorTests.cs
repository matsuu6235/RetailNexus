using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Controllers;
using RetailNexus.Api.Validators;
using RetailNexus.Resources;
using RetailNexus.Tests.Helpers;

namespace RetailNexus.Tests.Validators;

public class CreatePurchaseOrderValidatorTests
{
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly CreatePurchaseOrderRequestValidator _validator;

    public CreatePurchaseOrderValidatorTests()
    {
        _validator = new CreatePurchaseOrderRequestValidator(_localizer);
    }

    private static PurchaseOrdersController.CreatePurchaseOrderRequest ValidRequest()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            null,
            new List<PurchaseOrdersController.CreateDetailRequest>
            {
                new(Guid.NewGuid(), 10, 100m),
            });

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task SupplierId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with { SupplierId = Guid.Empty };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.SupplierId)
            .WithErrorMessage("仕入先は必須です。");
    }

    [Fact]
    public async Task StoreId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with { StoreId = Guid.Empty };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreId)
            .WithErrorMessage("発注元は必須です。");
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
        var request = ValidRequest() with { Details = new List<PurchaseOrdersController.CreateDetailRequest>() };

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
            Details = new List<PurchaseOrdersController.CreateDetailRequest>
            {
                new(productId, 10, 100m),
                new(productId, 5, 200m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Details)
            .WithErrorMessage("同一商品が複数行に含まれています。数量を変更してください。");
    }

    [Fact]
    public async Task Details_WhenDistinctProducts_ShouldPass()
    {
        var request = ValidRequest() with
        {
            Details = new List<PurchaseOrdersController.CreateDetailRequest>
            {
                new(Guid.NewGuid(), 10, 100m),
                new(Guid.NewGuid(), 5, 200m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Details);
    }

    [Fact]
    public async Task Detail_Quantity_WhenZero_ShouldFail()
    {
        var request = ValidRequest() with
        {
            Details = new List<PurchaseOrdersController.CreateDetailRequest>
            {
                new(Guid.NewGuid(), 0, 100m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e => e.ErrorMessage == "数量は1以上で入力してください。");
    }

    [Fact]
    public async Task Detail_UnitPrice_WhenNegative_ShouldFail()
    {
        var request = ValidRequest() with
        {
            Details = new List<PurchaseOrdersController.CreateDetailRequest>
            {
                new(Guid.NewGuid(), 10, -1m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e => e.ErrorMessage == "単価は0以上で入力してください。");
    }

    [Fact]
    public async Task Detail_ProductId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest() with
        {
            Details = new List<PurchaseOrdersController.CreateDetailRequest>
            {
                new(Guid.Empty, 10, 100m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.Errors.Should().Contain(e => e.ErrorMessage == "商品は必須です。");
    }
}

public class UpdatePurchaseOrderValidatorTests
{
    private readonly IStringLocalizer<SharedMessages> _localizer = MockLocalizerHelper.Create();
    private readonly UpdatePurchaseOrderRequestValidator _validator;

    public UpdatePurchaseOrderValidatorTests()
    {
        _validator = new UpdatePurchaseOrderRequestValidator(_localizer);
    }

    private static PurchaseOrdersController.UpdatePurchaseOrderRequest ValidRequest()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            new List<PurchaseOrdersController.UpdateDetailRequest>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 10, 100m),
            });

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Details_WhenDuplicateProducts_ShouldFail()
    {
        var productId = Guid.NewGuid();
        var request = ValidRequest() with
        {
            Details = new List<PurchaseOrdersController.UpdateDetailRequest>
            {
                new(Guid.NewGuid(), productId, 10, 100m),
                new(null, productId, 5, 200m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Details)
            .WithErrorMessage("同一商品が複数行に含まれています。数量を変更してください。");
    }

    [Fact]
    public async Task Details_WhenNewRowWithNullId_ShouldPass()
    {
        var request = ValidRequest() with
        {
            Details = new List<PurchaseOrdersController.UpdateDetailRequest>
            {
                new(null, Guid.NewGuid(), 10, 100m),
            }
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
