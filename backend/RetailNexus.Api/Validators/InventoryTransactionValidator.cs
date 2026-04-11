using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Controllers;
using RetailNexus.Domain.Enums;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public sealed class ManualTransactionRequestValidator : AbstractValidator<InventoryTransactionsController.ManualTransactionRequest>
{
    private static readonly HashSet<InventoryTransactionType> AllowedManualTypes = new()
    {
        InventoryTransactionType.Disposal,
        InventoryTransactionType.Adjustment,
        InventoryTransactionType.InitialStock,
    };

    public ManualTransactionRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "店舗"]);

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "商品"]);

        RuleFor(x => x.TransactionType)
            .Must(t => AllowedManualTypes.Contains(t))
            .WithMessage("手動登録できる取引種別は廃棄・棚卸調整・初期在庫のみです。");

        RuleFor(x => x.QuantityChange)
            .NotEqual(0).WithMessage(localizer["Validation_Required", "数量"]);

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage(localizer["Validation_MaxLength", "備考", 500])
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}
