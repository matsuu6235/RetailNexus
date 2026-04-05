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
        var categoryCode = productCategoryCode.Trim();
        var category = await _categoryRepo.GetByCodeAsync(categoryCode, ct)
            ?? throw new EntityNotFoundException("ProductCategory", categoryCode);

        var abbreviation = category.CategoryAbbreviation;
        var maxCode = await _productRepo.GetMaxProductCodeByPrefixAsync(abbreviation, ct);
        var productCode = CodeGenerator.NextProductCode(maxCode, abbreviation);

        var product = new Product(productCode, janCode.Trim(), productName.Trim(), price, cost, categoryCode, actorId);
        await _productRepo.AddAsync(product, ct);
        await _productRepo.SaveChangesAsync(ct);

        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid actorId, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("Product", id);

        product.Update(janCode.Trim(), productName.Trim(), price, cost, productCategoryCode.Trim(), actorId);
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
