using FluentValidation;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Api.Controllers;

namespace RetailNexus.Api.Validators;

public class SupplierRequestValidator<T> : AbstractValidator<T> where T : ISupplierRequest
{
    public SupplierRequestValidator(ISupplierRepository repo)
    {
        RuleFor(x => x.SupplierName)
            .NotEmpty().WithMessage("仕入先名は必須です。")
            .MaximumLength(50).WithMessage("仕入先名は50文字以内で入力してください。");

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

public sealed class CreateSupplierRequestValidator : SupplierRequestValidator<SuppliersController.CreateSupplierRequest>
{
    public CreateSupplierRequestValidator(ISupplierRepository repo) : base(repo) { }
}

public sealed class UpdateSupplierRequestValidator : SupplierRequestValidator<SuppliersController.UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator(ISupplierRepository repo) : base(repo) { }
}
