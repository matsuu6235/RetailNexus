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
    private readonly IProductCategoryRepository _productCategoryRepo;

    public ProductsController(IProductRepository productRepo, IProductCategoryRepository productCategoryRepo)
    {
        _productRepo = productRepo;
        _productCategoryRepo = productCategoryRepo;
    }

    public sealed record CreateProductRequest(string ProductCode, string JanCode, string ProductName, decimal Price, decimal Cost, string ProductCategoryCode);
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
        var productCode = req.ProductCode.Trim();
        var janCode = req.JanCode.Trim();
        var productName = req.ProductName.Trim();
        var producCategoryCode = req.ProductCategoryCode.Trim();

        if (string.IsNullOrWhiteSpace(productCode) || string.IsNullOrWhiteSpace(productName))
        {
            return BadRequest("ProductCode and ProductName are required.");
        }

        var existing = await _productRepo.GetByProductCodeAsync(productCode, ct);
        if (existing is not null)
        {
            return Conflict("ProductCode already exists.");
        }

        var productCategory = await _productCategoryRepo.GetByCodeAsync(producCategoryCode, ct);
        if (productCategory is null)
        {
            return BadRequest("ProductCategoryCode not found.");
        }

        if (!productCategory.IsActive)
        {
            return BadRequest("ProductCategoryCode is inactive.");
        }

        var product = new Product(
            productCode,
            janCode,
            productName,
            req.Price,
            req.Cost,
            producCategoryCode);

        await _productRepo.AddAsync(product, ct);
        await _productRepo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, Map(product));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

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
