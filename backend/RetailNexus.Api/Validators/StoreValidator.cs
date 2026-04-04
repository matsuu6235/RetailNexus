using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public class StoreRequestValidator<T> : AbstractValidator<T> where T : IStoreRequest
{
    protected StoreRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage(localizer["Validation_Required", "店舗名"])
            .MaximumLength(50).WithMessage(localizer["Validation_MaxLength", "店舗名", 50]);
    }

    protected IRuleBuilderOptions<T, Guid> AreaIdBaseRules(IStringLocalizer<SharedMessages> localizer)
    {
        return RuleFor(x => x.AreaId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "エリアID"]);
    }

    protected IRuleBuilderOptions<T, Guid> StoreTypeIdBaseRules(IStringLocalizer<SharedMessages> localizer)
    {
        return RuleFor(x => x.StoreTypeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "店舗種別ID"]);
    }
}

public sealed class CreateStoreRequestValidator : StoreRequestValidator<StoresController.CreateStoreRequest>
{
    public CreateStoreRequestValidator(
        IStoreRepository storeRepo,
        IAreaRepository areaRepo,
        IStoreTypeRepository storeTypeRepo,
        IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        AreaIdBaseRules(localizer)
            .MustAsync(async (areaId, ct) =>
            {
                var area = await areaRepo.GetByIdAsync(areaId, ct);
                return area?.IsActive is true;
            }).WithMessage(localizer["Validation_EntityNotFoundOrInactive", "エリア"]);

        StoreTypeIdBaseRules(localizer)
            .MustAsync(async (storeTypeId, ct) =>
            {
                var storeType = await storeTypeRepo.GetByIdAsync(storeTypeId, ct);
                return storeType?.IsActive is true;
            }).WithMessage(localizer["Validation_EntityNotFoundOrInactive", "店舗種別"]);
    }
}

public sealed class UpdateStoreRequestValidator : StoreRequestValidator<StoresController.UpdateStoreRequest>
{
    public UpdateStoreRequestValidator(
        IStoreRepository storeRepo,
        IAreaRepository areaRepo,
        IStoreTypeRepository storeTypeRepo,
        IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        AreaIdBaseRules(localizer)
            .MustAsync(async (areaId, ct) =>
            {
                var area = await areaRepo.GetByIdAsync(areaId, ct);
                return area is not null;
            }).WithMessage(localizer["Validation_EntityNotFound", "エリア"]);

        StoreTypeIdBaseRules(localizer)
            .MustAsync(async (storeTypeId, ct) =>
            {
                var storeType = await storeTypeRepo.GetByIdAsync(storeTypeId, ct);
                return storeType is not null;
            }).WithMessage(localizer["Validation_EntityNotFound", "店舗種別"]);
    }
}
