using FluentValidation;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;

namespace RetailNexus.Api.Validators;

public class StoreRequestValidator<T> : AbstractValidator<T> where T : IStoreRequest
{
    protected StoreRequestValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("店舗名は必須です。")
            .MaximumLength(50).WithMessage("店舗名は50文字以内で入力してください。");
    }

    protected IRuleBuilderOptions<T, Guid> AreaIdBaseRules()
    {
        return RuleFor(x => x.AreaId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("エリアIDは必須です。");
    }

    protected IRuleBuilderOptions<T, Guid> StoreTypeIdBaseRules()
    {
        return RuleFor(x => x.StoreTypeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗種別IDは必須です。");
    }
}

public sealed class CreateStoreRequestValidator : StoreRequestValidator<StoresController.CreateStoreRequest>
{
    public CreateStoreRequestValidator(
        IStoreRepository storeRepo,
        IAreaRepository areaRepo,
        IStoreTypeRepository storeTypeRepo)
    {
        AreaIdBaseRules()
            .MustAsync(async (areaId, ct) =>
            {
                var area = await areaRepo.GetByIdAsync(areaId, ct);
                return area?.IsActive is true;
            }).WithMessage("指定されたエリアが存在しないか、無効になっています。");

        StoreTypeIdBaseRules()
            .MustAsync(async (storeTypeId, ct) =>
            {
                var storeType = await storeTypeRepo.GetByIdAsync(storeTypeId, ct);
                return storeType?.IsActive is true;
            }).WithMessage("指定された店舗種別が存在しないか、無効になっています。");
    }
}

public sealed class UpdateStoreRequestValidator : StoreRequestValidator<StoresController.UpdateStoreRequest>
{
    public UpdateStoreRequestValidator(
        IStoreRepository storeRepo,
        IAreaRepository areaRepo,
        IStoreTypeRepository storeTypeRepo)
    {
        AreaIdBaseRules()
            .MustAsync(async (areaId, ct) =>
            {
                var area = await areaRepo.GetByIdAsync(areaId, ct);
                return area is not null;
            }).WithMessage("指定されたエリアが存在しません。");

        StoreTypeIdBaseRules()
            .MustAsync(async (storeTypeId, ct) =>
            {
                var storeType = await storeTypeRepo.GetByIdAsync(storeTypeId, ct);
                return storeType is not null;
            }).WithMessage("指定された店舗種別が存在しません。");
    }
}
