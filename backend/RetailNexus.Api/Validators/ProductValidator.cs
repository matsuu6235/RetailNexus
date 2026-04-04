using FluentValidation;
using Microsoft.Extensions.Localization;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Api.Controllers;
using RetailNexus.Resources;

namespace RetailNexus.Api.Validators;

public class ProductRequestValidator<T> : AbstractValidator<T> where T : IProductRequest
{
    public ProductRequestValidator(IProductRepository productRepo, IProductCategoryRepository categoryRepo, IStringLocalizer<SharedMessages> localizer)
    {
        RuleFor(x => x.JanCode)
            .Cascade(CascadeMode.Stop)
            .Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^\d+$"))
                .WithMessage(localizer["Validation_DigitsOnly", "JANコード"])
            .Length(13).WithMessage(localizer["Validation_ExactLength", "JANコード", 13])
            .When(x => !string.IsNullOrEmpty(x.JanCode));

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage(localizer["Validation_Required", "商品名"])
            .MaximumLength(200).WithMessage(localizer["Validation_MaxLength", "商品名", 200]);

        RuleFor(x => x.ProductCategoryCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation_Required", "商品カテゴリコード"])
            .MaximumLength(50).WithMessage(localizer["Validation_MaxLength", "商品カテゴリコード", 50])
            .MustAsync(async (code, ct) =>
            {
                var category = await categoryRepo.GetByCodeAsync(code.Trim(), ct);
                return category?.IsActive is true;
            }).WithMessage(localizer["Validation_EntityNotFoundOrInactive", "商品カテゴリ"]);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(localizer["Validation_MinValue", "売価", 0]);

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage(localizer["Validation_MinValue", "原価", 0]);
    }
}

public sealed class CreateProductRequestValidator : ProductRequestValidator<ProductsController.CreateProductRequest>
{
    public CreateProductRequestValidator(IProductRepository productRepo, IProductCategoryRepository categoryRepo, IStringLocalizer<SharedMessages> localizer)
        : base(productRepo, categoryRepo, localizer) { }
}

public sealed class UpdateProductRequestValidator : ProductRequestValidator<ProductsController.UpdateProductRequest>
{
    public UpdateProductRequestValidator(IProductRepository productRepo, IProductCategoryRepository categoryRepo, IStringLocalizer<SharedMessages> localizer)
        : base(productRepo, categoryRepo, localizer) { }
}
