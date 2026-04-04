using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public class ProductCategoryRequestValidator<T> : AbstractValidator<T> where T : IProductCategoryRequest
{
    protected ProductCategoryRequestValidator(IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.ProductCategoryName)
            .NotEmpty().WithMessage(localizer["Validation_Required", "商品カテゴリ名"])
            .MaximumLength(30).WithMessage(localizer["Validation_MaxLength", "商品カテゴリ名", 30]);
    }

    protected IRuleBuilderOptions<T, string> ProductCategoryCdBaseRules(IStringLocalizer<SharedMessages> localizer)
    {
        return RuleFor(x => x.ProductCategoryCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "商品カテゴリコード"])
            .MaximumLength(3).WithMessage(localizer["Validation_MaxLength", "商品カテゴリコード", 3])
            .Matches(@"^\d+$").WithMessage(localizer["Validation_DigitsOnly", "商品カテゴリコード"]);
    }

    protected IRuleBuilderOptions<T, string> CategoryAbbreviationBaseRules(IStringLocalizer<SharedMessages> localizer)
    {
        return RuleFor(x => x.CategoryAbbreviation)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "カテゴリ略称"])
            .Matches(@"^[A-Za-z]{2,5}$").WithMessage(localizer["Validation_AlphaRange", "カテゴリ略称", 2, 5]);
    }
}

public sealed class CreateProductCategoryRequestValidator : ProductCategoryRequestValidator<ProductCategoriesController.CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator(IProductCategoryRepository repo, IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        ProductCategoryCdBaseRules(localizer)
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage(localizer["Validation_Duplicate", "商品カテゴリコード"]);

        CategoryAbbreviationBaseRules(localizer)
            .MustAsync(async (abbreviation, ct) =>
            {
                var existing = await repo.GetByAbbreviationAsync(abbreviation.Trim().ToUpperInvariant(), ct);
                return existing is null;
            }).WithMessage(localizer["Validation_Duplicate", "カテゴリ略称"]);
    }
}

public sealed class UpdateProductCategoryRequestValidator : ProductCategoryRequestValidator<ProductCategoriesController.UpdateProductCategoryRequest>
{
    public UpdateProductCategoryRequestValidator(IProductCategoryRepository repo, IStringLocalizer<SharedMessages> localizer) : base(localizer)
    {
        ProductCategoryCdBaseRules(localizer)
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.ProductCategoryId == entityId;
            }).WithMessage(localizer["Validation_Duplicate", "商品カテゴリコード"]);

        CategoryAbbreviationBaseRules(localizer)
            .MustAsync(async (request, abbreviation, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByAbbreviationExcludingAsync(abbreviation.Trim().ToUpperInvariant(), entityId, ct);
                return existing is null;
            }).WithMessage(localizer["Validation_Duplicate", "カテゴリ略称"]);
    }
}

public sealed class ReorderProductCategoriesRequestValidator : AbstractValidator<ProductCategoriesController.ReorderProductCategoriesRequest>
{
    public ReorderProductCategoriesRequestValidator(IProductCategoryRepository repo, IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.ProductCategoryIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "商品カテゴリIDリスト"])
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage(localizer["Validation_ListDuplicate", "商品カテゴリ"])
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();
                var entities = await repo.GetByIdsAsync(distinctIds, ct);
                return entities.Count == distinctIds.Length;
            }).WithMessage(localizer["Validation_ListInvalidIds", "商品カテゴリ"]);
    }
}
