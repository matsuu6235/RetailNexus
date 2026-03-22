using FluentValidation;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;

namespace RetailNexus.Api.Validators;

public sealed class CreateProductCategoryRequestValidator : AbstractValidator<ProductCategoriesController.CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator(IProductCategoryRepository repo)
    {
        RuleFor(x => x.ProductCategoryCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("商品カテゴリコードは必須です。")
            .MaximumLength(30).WithMessage("商品カテゴリコードは30文字以内で入力してください。")
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage("この商品カテゴリコードは既に使用されています。");

        RuleFor(x => x.ProductCategoryName)
            .NotEmpty().WithMessage("商品カテゴリ名は必須です。")
            .MaximumLength(100).WithMessage("商品カテゴリ名は100文字以内で入力してください。");
    }
}

public sealed class UpdateProductCategoryRequestValidator : AbstractValidator<ProductCategoriesController.UpdateProductCategoryRequest>
{
    public UpdateProductCategoryRequestValidator(IProductCategoryRepository repo)
    {
        RuleFor(x => x.ProductCategoryCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("商品カテゴリコードは必須です。")
            .MaximumLength(30).WithMessage("商品カテゴリコードは30文字以内で入力してください。")
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.ProductCategoryId == entityId;
            }).WithMessage("この商品カテゴリコードは既に使用されています。");

        RuleFor(x => x.ProductCategoryName)
            .NotEmpty().WithMessage("商品カテゴリ名は必須です。")
            .MaximumLength(100).WithMessage("商品カテゴリ名は100文字以内で入力してください。");
    }
}

public sealed class ReorderProductCategoriesRequestValidator : AbstractValidator<ProductCategoriesController.ReorderProductCategoriesRequest>
{
    public ReorderProductCategoriesRequestValidator(IProductCategoryRepository repo)
    {
        RuleFor(x => x.ProductCategoryIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("商品カテゴリIDリストは必須です。")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("商品カテゴリIDリストに重複があります。")
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();
                var entities = await repo.GetByIdsAsync(distinctIds, ct);
                return entities.Count == distinctIds.Length;
            }).WithMessage("存在しない商品カテゴリIDが含まれています。");
    }
}
