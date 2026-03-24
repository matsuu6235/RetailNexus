using FluentValidation;
using RetailNexus.Application.Interfaces;

namespace RetailNexus.Api.Validators;

public sealed class CreateStoreTypeRequestValidator : AbstractValidator<StoreTypesController.CreateStoreTypeRequest>
{
    public CreateStoreTypeRequestValidator(IStoreTypeRepository repo)
    {
        RuleFor(x => x.StoreTypeCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗種別コードは必須です。")
            .MaximumLength(2).WithMessage("店舗種別コードは2文字以内で入力してください。")
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage("この店舗種別コードは既に使用されています。");

        RuleFor(x => x.StoreTypeName)
            .NotEmpty().WithMessage("店舗種別名は必須です。")
            .MaximumLength(20).WithMessage("店舗種別名は20文字以内で入力してください。");
    }
}

public sealed class UpdateStoreTypeRequestValidator : AbstractValidator<StoreTypesController.UpdateStoreTypeRequest>
{
    public UpdateStoreTypeRequestValidator(IStoreTypeRepository repo)
    {
        RuleFor(x => x.StoreTypeCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗種別コードは必須です。")
            .MaximumLength(2).WithMessage("店舗種別コードは2文字以内で入力してください。")
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.StoreTypeId == entityId;
            }).WithMessage("この店舗種別コードは既に使用されています。");

        RuleFor(x => x.StoreTypeName)
            .NotEmpty().WithMessage("店舗種別名は必須です。")
            .MaximumLength(20).WithMessage("店舗種別名は20文字以内で入力してください。");
    }
}

public sealed class ReorderStoreTypesRequestValidator : AbstractValidator<StoreTypesController.ReorderStoreTypesRequest>
{
    public ReorderStoreTypesRequestValidator(IStoreTypeRepository repo)
    {
        RuleFor(x => x.StoreTypeIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("店舗種別IDリストは必須です。")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("店舗種別IDリストに重複があります。")
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();
                var entities = await repo.GetByIdsAsync(distinctIds, ct);
                return entities.Count == distinctIds.Length;
            }).WithMessage("存在しない店舗種別IDが含まれています。");
    }
}
