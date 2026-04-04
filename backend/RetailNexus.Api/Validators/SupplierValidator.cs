using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Api.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public class SupplierRequestValidator<T> : AbstractValidator<T> where T : ISupplierRequest
{
    public SupplierRequestValidator(ISupplierRepository repo, IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.SupplierName)
            .NotEmpty().WithMessage(localizer["Validation_Required", "仕入先名"])
            .MaximumLength(50).WithMessage(localizer["Validation_MaxLength", "仕入先名", 50]);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage(localizer["Validation_MaxLength", "電話番号", 20])
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(255).WithMessage(localizer["Validation_MaxLength", "メールアドレス", 255])
            .EmailAddress().WithMessage(localizer["Validation_EmailFormat"])
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public sealed class CreateSupplierRequestValidator : SupplierRequestValidator<SuppliersController.CreateSupplierRequest>
{
    public CreateSupplierRequestValidator(ISupplierRepository repo, IStringLocalizer<SharedMessages> localizer) : base(repo, localizer) { }
}

public sealed class UpdateSupplierRequestValidator : SupplierRequestValidator<SuppliersController.UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator(ISupplierRepository repo, IStringLocalizer<SharedMessages> localizer) : base(repo, localizer) { }
}
