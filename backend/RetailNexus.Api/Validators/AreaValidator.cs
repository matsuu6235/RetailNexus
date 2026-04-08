using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public class AreaRequestValidator<T> : AbstractValidator<T> where T : IAreaRequest
{
    protected AreaRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.AreaName)
            .NotEmpty().WithMessage(localizer["Validation_Required", "エリア名"])
            .MaximumLength(20).WithMessage(localizer["Validation_MaxLength", "エリア名", 20]);
    }

    protected IRuleBuilderOptions<T, string> AreaCdBaseRules(IStringLocalizer<SharedMessages> localizer)
    {
        return RuleFor(x => x.AreaCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "エリアコード"])
            .MaximumLength(2).WithMessage(localizer["Validation_MaxLength", "エリアコード", 2]);
    }
}

public sealed class CreateAreaRequestValidator : AreaRequestValidator<AreasController.CreateAreaRequest>
{
    public CreateAreaRequestValidator(IAreaRepository repo, IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        AreaCdBaseRules(localizer)
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code, ct);
                return existing is null;
            }).WithMessage(localizer["Validation_Duplicate", "エリアコード"]);
    }
}

public sealed class UpdateAreaRequestValidator : AreaRequestValidator<AreasController.UpdateAreaRequest>
{
    public UpdateAreaRequestValidator(IAreaRepository repo, IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        AreaCdBaseRules(localizer)
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code, ct);
                return existing is null || existing.AreaId == entityId;
            }).WithMessage(localizer["Validation_Duplicate", "エリアコード"]);
    }
}

public sealed class ReorderAreasRequestValidator : AbstractValidator<AreasController.ReorderAreasRequest>
{
    public ReorderAreasRequestValidator(IAreaRepository repo, IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.AreaIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "エリアIDリスト"])
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage(localizer["Validation_ListDuplicate", "エリア"])
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();
                var entities = await repo.GetByIdsAsync(distinctIds, ct);
                return entities.Count == distinctIds.Length;
            }).WithMessage(localizer["Validation_ListInvalidIds", "エリア"]);
    }
}
