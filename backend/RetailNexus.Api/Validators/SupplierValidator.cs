using FluentValidation;
using RetailNexus.Application.Interfaces;
using RetailNexus.Api.Controllers;

namespace RetailNexus.Api.Validators;

public sealed class CreateSupplierRequestValidator : AbstractValidator<SuppliersController.CreateSupplierRequest>
{
    public CreateSupplierRequestValidator(ISupplierRepository repo)
    {
        RuleFor(x => x.SupplierCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("仕入先コードは必須です。")
            .MaximumLength(30).WithMessage("仕入先コードは30文字以内で入力してください。")
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetBySupplierCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage("この仕入先コードは既に使用されています。");

        RuleFor(x => x.SupplierName)
            .NotEmpty().WithMessage("仕入先名は必須です。")
            .MaximumLength(100).WithMessage("仕入先名は100文字以内で入力してください。");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("電話番号は20文字以内で入力してください。")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(255).WithMessage("メールアドレスは255文字以内で入力してください。")
            .EmailAddress().WithMessage("メールアドレスの形式が正しくありません。")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public sealed class UpdateSupplierRequestValidator : AbstractValidator<SuppliersController.UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator(ISupplierRepository repo)
    {
        RuleFor(x => x.SupplierCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("仕入先コードは必須です。")
            .MaximumLength(30).WithMessage("仕入先コードは30文字以内で入力してください。")
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetBySupplierCodeAsync(code.Trim(), ct);
                return existing is null || existing.SupplierId == entityId;
            }).WithMessage("この仕入先コードは既に使用されています。");

        RuleFor(x => x.SupplierName)
            .NotEmpty().WithMessage("仕入先名は必須です。")
            .MaximumLength(100).WithMessage("仕入先名は100文字以内で入力してください。");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("電話番号は20文字以内で入力してください。")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(255).WithMessage("メールアドレスは255文字以内で入力してください。")
            .EmailAddress().WithMessage("メールアドレスの形式が正しくありません。")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
