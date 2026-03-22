using FluentValidation;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;

namespace RetailNexus.Api.Validators;

public sealed class CreateStoreRequestValidator : AbstractValidator<StoresController.CreateStoreRequest>
{
    public CreateStoreRequestValidator(
        IStoreRepository storeRepo,
        IAreaRepository areaRepo,
        IStoreTypeRepository storeTypeRepo)
    {
        RuleFor(x => x.StoreCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗コードは必須です。")
            .MaximumLength(6).WithMessage("店舗コードは6文字以内で入力してください。")
            .MustAsync(async (code, ct) =>
            {
                var existing = await storeRepo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage("この店舗コードは既に使用されています。");

        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("店舗名は必須です。")
            .MaximumLength(100).WithMessage("店舗名は100文字以内で入力してください。");

        RuleFor(x => x.AreaId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("エリアIDは必須です。")
            .MustAsync(async (areaId, ct) =>
            {
                var area = await areaRepo.GetByIdAsync(areaId, ct);
                return area?.IsActive is true;
            }).WithMessage("指定されたエリアが存在しないか、無効になっています。");

        RuleFor(x => x.StoreTypeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗種別IDは必須です。")
            .MustAsync(async (storeTypeId, ct) =>
            {
                var storeType = await storeTypeRepo.GetByIdAsync(storeTypeId, ct);
                return storeType?.IsActive is true;
            }).WithMessage("指定された店舗種別が存在しないか、無効になっています。");
    }
}

public sealed class UpdateStoreRequestValidator : AbstractValidator<StoresController.UpdateStoreRequest>
{
    public UpdateStoreRequestValidator(
        IStoreRepository storeRepo,
        IAreaRepository areaRepo,
        IStoreTypeRepository storeTypeRepo)
    {
        RuleFor(x => x.StoreCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗コードは必須です。")
            .MaximumLength(6).WithMessage("店舗コードは6文字以内で入力してください。")
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await storeRepo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.StoreId == entityId;
            }).WithMessage("この店舗コードは既に使用されています。");

        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("店舗名は必須です。")
            .MaximumLength(100).WithMessage("店舗名は100文字以内で入力してください。");

        RuleFor(x => x.AreaId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("エリアIDは必須です。")
            .MustAsync(async (areaId, ct) =>
            {
                var area = await areaRepo.GetByIdAsync(areaId, ct);
                return area is not null;
            }).WithMessage("指定されたエリアが存在しません。");

        RuleFor(x => x.StoreTypeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗種別IDは必須です。")
            .MustAsync(async (storeTypeId, ct) =>
            {
                var storeType = await storeTypeRepo.GetByIdAsync(storeTypeId, ct);
                return storeType is not null;
            }).WithMessage("指定された店舗種別が存在しません。");
    }
}
