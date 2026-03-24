using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepo;
    private readonly IProductCategoryRepository _categoryRepo;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductsController(
        IProductRepository productRepo,
        IProductCategoryRepository categoryRepo,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public sealed record CreateProductRequest(string JanCode, string ProductName, decimal Price, decimal Cost, string ProductCategoryCode);
    public sealed record UpdateProductRequest(string JanCode, string ProductName, decimal Price, decimal Cost, string ProductCategoryCode, bool IsActive);
    public sealed record ProductResponse(
        Guid Id,
        string ProductCode,
        string JanCode,
        string ProductName,
        string ProductCategoryCode,
        decimal Price,
        decimal Cost,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        DateTimeOffset CreatedAt);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var categoryCode = req.ProductCategoryCode.Trim();
        var category = await _categoryRepo.GetByCodeAsync(categoryCode, ct);
        if (category is null)
            return BadRequest(new { ProductCategoryCode = new[] { "指定された商品カテゴリが存在しません。" } });

        var abbreviation = category.CategoryAbbreviation;
        var maxCode = await _productRepo.GetMaxProductCodeByPrefixAsync(abbreviation, ct);
        var nextSeq = 1;
        if (maxCode is not null)
        {
            var numericPart = maxCode[(abbreviation.Length + 1)..];
            nextSeq = int.Parse(numericPart) + 1;
        }
        var productCode = $"{abbreviation}-{nextSeq:D6}";

        var product = new Product(
            productCode,
            req.JanCode.Trim(),
            req.ProductName.Trim(),
            req.Price,
            req.Cost,
            categoryCode);

        await _productRepo.AddAsync(product, ct);
        await _productRepo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, Map(product));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound();

        var validationContext = new FluentValidation.ValidationContext<UpdateProductRequest>(req);
        validationContext.RootContextData["productId"] = id;
        var validation = await _updateValidator.ValidateAsync(validationContext, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        product.Update(
            req.JanCode.Trim(),
            req.ProductName.Trim(),
            req.Price,
            req.Cost,
            req.ProductCategoryCode.Trim(),
            req.IsActive);

        await _productRepo.SaveChangesAsync(ct);

        return Ok(Map(product));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound();

        return Ok(Map(product));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? productCode,
        [FromQuery] string? janCode,
        [FromQuery] string? productName,
        [FromQuery] string? productCategoryCode,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var skip = (page - 1) * pageSize;

        var total = await _productRepo.CountAsync(productCode, janCode, productName, productCategoryCode, isActive, ct);
        var items = await _productRepo.ListAsync(productCode, janCode, productName, productCategoryCode, isActive, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(Map)
        });
    }

    private static ProductResponse Map(Product x)
        => new(
            x.Id,
            x.ProductCode,
            x.JanCode,
            x.ProductName,
            x.ProductCategoryCode,
            x.Price,
            x.Cost,
            x.IsActive,
            x.UpdatedAt,
            x.CreatedAt);
}
