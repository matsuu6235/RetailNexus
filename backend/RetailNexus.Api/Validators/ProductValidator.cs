using FluentValidation;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Api.Controllers;

namespace RetailNexus.Api.Validators;

public class ProductRequestValidator<T> : AbstractValidator<T> where T : IProductRequest
{
    public ProductRequestValidator(IProductRepository productRepo, IProductCategoryRepository categoryRepo)
    {
        RuleFor(x => x.JanCode)
            .Cascade(CascadeMode.Stop)
            .Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^\d+$"))
                .WithMessage("JANコードは数字のみ入力できます。")
            .Length(13).WithMessage("JANコードは13桁で入力してください。")
            .When(x => !string.IsNullOrEmpty(x.JanCode));

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("商品名は必須です。")
            .MaximumLength(200).WithMessage("商品名は200文字以内で入力してください。");

        RuleFor(x => x.ProductCategoryCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("商品カテゴリコードは必須です。")
            .MaximumLength(50).WithMessage("商品カテゴリコードは50文字以内で入力してください。")
            .MustAsync(async (code, ct) =>
            {
                var category = await categoryRepo.GetByCodeAsync(code.Trim(), ct);
                return category?.IsActive is true;
            }).WithMessage("指定された商品カテゴリが存在しないか、無効になっています。");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("売価は0以上で入力してください。");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("原価は0以上で入力してください。");
    }
}

public sealed class CreateProductRequestValidator : ProductRequestValidator<ProductsController.CreateProductRequest>
{
    public CreateProductRequestValidator(IProductRepository productRepo, IProductCategoryRepository categoryRepo)
        : base(productRepo, categoryRepo) { }
}

public sealed class UpdateProductRequestValidator : ProductRequestValidator<ProductsController.UpdateProductRequest>
{
    public UpdateProductRequestValidator(IProductRepository productRepo, IProductCategoryRepository categoryRepo)
        : base(productRepo, categoryRepo) { }
}
