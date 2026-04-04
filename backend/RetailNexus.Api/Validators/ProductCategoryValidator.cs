using FluentValidation;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;

namespace RetailNexus.Api.Validators;

public class ProductCategoryRequestValidator<T> : AbstractValidator<T> where T : IProductCategoryRequest
{
    protected ProductCategoryRequestValidator()
    {
        RuleFor(x => x.ProductCategoryName)
            .NotEmpty().WithMessage("商品カテゴリ名は必須です。")
            .MaximumLength(30).WithMessage("商品カテゴリ名は30文字以内で入力してください。");
    }

    protected IRuleBuilderOptions<T, string> ProductCategoryCdBaseRules()
    {
        return RuleFor(x => x.ProductCategoryCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("商品カテゴリコードは必須です。")
            .MaximumLength(3).WithMessage("商品カテゴリコードは3文字以内で入力してください。")
            .Matches(@"^\d+$").WithMessage("商品カテゴリコードは数字のみ入力できます。");
    }

    protected IRuleBuilderOptions<T, string> CategoryAbbreviationBaseRules()
    {
        return RuleFor(x => x.CategoryAbbreviation)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("カテゴリ略称は必須です。")
            .Matches(@"^[A-Za-z]{2,5}$").WithMessage("カテゴリ略称は英字2〜5文字で入力してください。");
    }
}

public sealed class CreateProductCategoryRequestValidator : ProductCategoryRequestValidator<ProductCategoriesController.CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator(IProductCategoryRepository repo)
    {
        ProductCategoryCdBaseRules()
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage("この商品カテゴリコードは既に使用されています。");

        CategoryAbbreviationBaseRules()
            .MustAsync(async (abbreviation, ct) =>
            {
                var existing = await repo.GetByAbbreviationAsync(abbreviation.Trim().ToUpperInvariant(), ct);
                return existing is null;
            }).WithMessage("このカテゴリ略称は既に使用されています。");
    }
}

public sealed class UpdateProductCategoryRequestValidator : ProductCategoryRequestValidator<ProductCategoriesController.UpdateProductCategoryRequest>
{
    public UpdateProductCategoryRequestValidator(IProductCategoryRepository repo)
    {
        ProductCategoryCdBaseRules()
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.ProductCategoryId == entityId;
            }).WithMessage("この商品カテゴリコードは既に使用されています。");

        CategoryAbbreviationBaseRules()
            .MustAsync(async (request, abbreviation, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByAbbreviationExcludingAsync(abbreviation.Trim().ToUpperInvariant(), entityId, ct);
                return existing is null;
            }).WithMessage("このカテゴリ略称は既に使用されています。");
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
