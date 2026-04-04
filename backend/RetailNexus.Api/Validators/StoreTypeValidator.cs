using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public class StoreTypeRequestValidator<T> : AbstractValidator<T> where T : IStoreTypeRequest
{
    protected StoreTypeRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.StoreTypeName)
            .NotEmpty().WithMessage(localizer["Validation_Required", "店舗種別名"])
            .MaximumLength(20).WithMessage(localizer["Validation_MaxLength", "店舗種別名", 20]);
    }

    protected IRuleBuilderOptions<T, string> StoreTypeCdBaseRules(IStringLocalizer<SharedMessages> localizer)
    {
        return RuleFor(x => x.StoreTypeCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "店舗種別コード"])
            .MaximumLength(2).WithMessage(localizer["Validation_MaxLength", "店舗種別コード", 2]);
    }
}

public sealed class CreateStoreTypeRequestValidator : StoreTypeRequestValidator<StoreTypesController.CreateStoreTypeRequest>
{
    public CreateStoreTypeRequestValidator(IStoreTypeRepository repo, IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        StoreTypeCdBaseRules(localizer)
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage(localizer["Validation_Duplicate", "店舗種別コード"]);
    }
}

public sealed class UpdateStoreTypeRequestValidator : StoreTypeRequestValidator<StoreTypesController.UpdateStoreTypeRequest>
{
    public UpdateStoreTypeRequestValidator(IStoreTypeRepository repo, IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        StoreTypeCdBaseRules(localizer)
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.StoreTypeId == entityId;
            }).WithMessage(localizer["Validation_Duplicate", "店舗種別コード"]);
    }
}

public sealed class ReorderStoreTypesRequestValidator : AbstractValidator<StoreTypesController.ReorderStoreTypesRequest>
{
    public ReorderStoreTypesRequestValidator(IStoreTypeRepository repo, IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.StoreTypeIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "店舗種別IDリスト"])
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage(localizer["Validation_ListDuplicate", "店舗種別"])
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();
                var entities = await repo.GetByIdsAsync(distinctIds, ct);
                return entities.Count == distinctIds.Length;
            }).WithMessage(localizer["Validation_ListInvalidIds", "店舗種別"]);
    }
}
