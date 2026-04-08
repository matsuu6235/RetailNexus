using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IProductCategoryRepository _categoryRepo;

    public ProductService(IProductRepository productRepo, IProductCategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<Product> CreateAsync(string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid actorId, CancellationToken ct)
    {
        var trimmedProductCategoryCode = productCategoryCode.Trim();
        var trimmedJanCode = janCode.Trim();
        var trimmedProductName = productName.Trim();

        var category = await _categoryRepo.GetByCodeAsync(trimmedProductCategoryCode, ct)
            ?? throw new EntityNotFoundException("ProductCategory", trimmedProductCategoryCode);

        var categoryAbbreviation = category.CategoryAbbreviation;
        var maxCode = await _productRepo.GetMaxProductCodeByPrefixAsync(categoryAbbreviation, ct);
        var productCode = CodeGenerator.NextProductCode(maxCode, categoryAbbreviation);

        var product = new Product(productCode, trimmedJanCode, trimmedProductName, price, cost, trimmedProductCategoryCode, actorId);
        await _productRepo.AddAsync(product, ct);
        await _productRepo.SaveChangesAsync(ct);

        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid actorId, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Product", id);

        var trimmedJanCode = janCode.Trim();
        var trimmedProductName = productName.Trim();
        var trimmedProductCategoryCode = productCategoryCode.Trim();
        product.Update(trimmedJanCode, trimmedProductName, price, cost, trimmedProductCategoryCode, actorId);
        await _productRepo.SaveChangesAsync(ct);

        return product;
    }

    public async Task<Product> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Product", id);

        product.SetActivation(isActive, actorId);
        await _productRepo.SaveChangesAsync(ct);

        return product;
    }
}
