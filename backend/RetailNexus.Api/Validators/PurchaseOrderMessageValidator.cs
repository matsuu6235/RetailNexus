using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public sealed class SendMessageRequestValidator : AbstractValidator<PurchaseOrderMessagesController.SendMessageRequest>
{
    public SendMessageRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.Body)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "メッセージ"])
            .MaximumLength(500).WithMessage(localizer["Validation_MaxLength", "メッセージ", 500]);
    }
}
