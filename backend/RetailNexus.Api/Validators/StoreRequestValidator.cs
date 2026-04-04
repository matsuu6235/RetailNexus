using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public sealed class CreateStoreRequestRequestValidator : AbstractValidator<StoreRequestsController.CreateStoreRequestRequest>
{
    public CreateStoreRequestRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.FromStoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "依頼元"]);

        RuleFor(x => x.ToStoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "依頼先"]);

        RuleFor(x => x)
            .Must(x => x.FromStoreId != x.ToStoreId || x.FromStoreId == Guid.Empty)
            .WithMessage(localizer["StoreRequest_SameStore"]);

        RuleFor(x => x.RequestDate)
            .NotEmpty().WithMessage(localizer["Validation_Required", "依頼日"]);

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
            }).WithMessage(localizer["StoreRequest_DuplicateProduct"]);

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .NotEmpty().WithMessage(localizer["Validation_Required", "商品"]);

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage(localizer["Validation_GreaterThan", "数量", 1]);
        });
    }
}

public sealed class UpdateStoreRequestRequestValidator : AbstractValidator<StoreRequestsController.UpdateStoreRequestRequest>
{
    public UpdateStoreRequestRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.FromStoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "依頼元"]);

        RuleFor(x => x.ToStoreId)
            .NotEmpty().WithMessage(localizer["Validation_Required", "依頼先"]);

        RuleFor(x => x)
            .Must(x => x.FromStoreId != x.ToStoreId || x.FromStoreId == Guid.Empty)
            .WithMessage(localizer["StoreRequest_SameStore"]);

        RuleFor(x => x.RequestDate)
            .NotEmpty().WithMessage(localizer["Validation_Required", "依頼日"]);

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
            }).WithMessage(localizer["StoreRequest_DuplicateProduct"]);

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .NotEmpty().WithMessage(localizer["Validation_Required", "商品"]);

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage(localizer["Validation_GreaterThan", "数量", 1]);
        });
    }
}
