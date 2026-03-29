using FluentValidation;
using RetailNexus.Api.Controllers;

namespace RetailNexus.Api.Validators;

public sealed class CreateStoreRequestRequestValidator : AbstractValidator<StoreRequestsController.CreateStoreRequestRequest>
{
    public CreateStoreRequestRequestValidator()
    {
        RuleFor(x => x.FromStoreId)
            .NotEmpty().WithMessage("依頼元は必須です。");

        RuleFor(x => x.ToStoreId)
            .NotEmpty().WithMessage("依頼先は必須です。");

        RuleFor(x => x)
            .Must(x => x.FromStoreId != x.ToStoreId || x.FromStoreId == Guid.Empty)
            .WithMessage("依頼元と依頼先は異なる店舗を選択してください。");

        RuleFor(x => x.RequestDate)
            .NotEmpty().WithMessage("依頼日は必須です。");

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
        });
    }
}

public sealed class UpdateStoreRequestRequestValidator : AbstractValidator<StoreRequestsController.UpdateStoreRequestRequest>
{
    public UpdateStoreRequestRequestValidator()
    {
        RuleFor(x => x.FromStoreId)
            .NotEmpty().WithMessage("依頼元は必須です。");

        RuleFor(x => x.ToStoreId)
            .NotEmpty().WithMessage("依頼先は必須です。");

        RuleFor(x => x)
            .Must(x => x.FromStoreId != x.ToStoreId || x.FromStoreId == Guid.Empty)
            .WithMessage("依頼元と依頼先は異なる店舗を選択してください。");

        RuleFor(x => x.RequestDate)
            .NotEmpty().WithMessage("依頼日は必須です。");

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
        });
    }
}
