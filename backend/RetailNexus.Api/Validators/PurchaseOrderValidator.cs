using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public sealed class CreatePurchaseOrderRequestValidator : AbstractValidator<PurchaseOrdersController.CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "仕入先"]);

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "発注元"]);

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage(localizer["Validation_Required", "発注日"]);

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage(localizer["Validation_MaxLength", "備考", 500])
            .When(x => !string.IsNullOrEmpty(x.Note));

        RuleFor(x => x.Details)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_ListMinCount", "明細"])
            .Must(details =>
            {
                var productIds = details.Select(d => d.ProductId).Where(id => id != Guid.Empty).ToList();
                return productIds.Count == productIds.Distinct().Count();
            }).WithMessage(localizer["PurchaseOrder_DuplicateProduct"]);

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .NotEmpty().WithMessage(localizer["Validation_Required", "商品"]);

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage(localizer["Validation_GreaterThan", "数量", 1]);

            detail.RuleFor(d => d.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["Validation_MinValue", "単価", 0]);
        });
    }
}

public sealed class UpdatePurchaseOrderRequestValidator : AbstractValidator<PurchaseOrdersController.UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "仕入先"]);

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "発注元"]);

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage(localizer["Validation_Required", "発注日"]);

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage(localizer["Validation_MaxLength", "備考", 500])
            .When(x => !string.IsNullOrEmpty(x.Note));

        RuleFor(x => x.Details)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_ListMinCount", "明細"])
            .Must(details =>
            {
                var productIds = details.Select(d => d.ProductId).Where(id => id != Guid.Empty).ToList();
                return productIds.Count == productIds.Distinct().Count();
            }).WithMessage(localizer["PurchaseOrder_DuplicateProduct"]);

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .NotEmpty().WithMessage(localizer["Validation_Required", "商品"]);

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage(localizer["Validation_GreaterThan", "数量", 1]);

            detail.RuleFor(d => d.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["Validation_MinValue", "単価", 0]);
        });
    }
}
