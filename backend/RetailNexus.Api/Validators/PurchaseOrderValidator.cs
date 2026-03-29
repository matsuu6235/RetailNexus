using FluentValidation;
using RetailNexus.Api.Controllers;

namespace RetailNexus.Api.Validators;

public sealed class CreatePurchaseOrderRequestValidator : AbstractValidator<PurchaseOrdersController.CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("仕入先は必須です。");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("発注元は必須です。");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("発注日は必須です。");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("備考は500文字以内で入力してください。")
            .When(x => !string.IsNullOrEmpty(x.Note));

        RuleFor(x => x.Details)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("明細を1行以上入力してください。")
            .Must(details =>
            {
                var productIds = details.Select(d => d.ProductId).Where(id => id != Guid.Empty).ToList();
                return productIds.Count == productIds.Distinct().Count();
            }).WithMessage("同一商品が複数行に含まれています。数量を変更してください。");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .NotEmpty().WithMessage("商品は必須です。");

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage("数量は1以上で入力してください。");

            detail.RuleFor(d => d.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("単価は0以上で入力してください。");
        });
    }
}

public sealed class UpdatePurchaseOrderRequestValidator : AbstractValidator<PurchaseOrdersController.UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("仕入先は必須です。");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("発注元は必須です。");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("発注日は必須です。");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("備考は500文字以内で入力してください。")
            .When(x => !string.IsNullOrEmpty(x.Note));

        RuleFor(x => x.Details)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("明細を1行以上入力してください。")
            .Must(details =>
            {
                var productIds = details.Select(d => d.ProductId).Where(id => id != Guid.Empty).ToList();
                return productIds.Count == productIds.Distinct().Count();
            }).WithMessage("同一商品が複数行に含まれています。数量を変更してください。");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .NotEmpty().WithMessage("商品は必須です。");

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage("数量は1以上で入力してください。");

            detail.RuleFor(d => d.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("単価は0以上で入力してください。");
        });
    }
}
