using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Api.Contracts;
using RetailNexus.Api.Controllers;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[Route("api/[controller]")]
[Authorize]
public sealed class ProductCategoriesController : BaseController
{
    private readonly IProductCategoryRepository _repo;
    private readonly IProductCategoryService _service;
    private readonly IValidator<CreateProductCategoryRequest> _createValidator;
    private readonly IValidator<UpdateProductCategoryRequest> _updateValidator;
    private readonly IValidator<ReorderProductCategoriesRequest> _reorderValidator;

    public ProductCategoriesController(
        IProductCategoryRepository repo,
        IProductCategoryService service,
        IValidator<CreateProductCategoryRequest> createValidator,
        IValidator<UpdateProductCategoryRequest> updateValidator,
        IValidator<ReorderProductCategoriesRequest> reorderValidator)
    {
        _repo = repo;
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    public sealed record CreateProductCategoryRequest(string ProductCategoryCd, string CategoryAbbreviation, string ProductCategoryName, bool IsActive = true) : IProductCategoryRequest;
    public sealed record UpdateProductCategoryRequest(string ProductCategoryCd, string CategoryAbbreviation, string ProductCategoryName) : IProductCategoryRequest;
    public sealed record ReorderProductCategoriesRequest(IReadOnlyList<Guid> ProductCategoryIds);
    public sealed record ProductCategoryResponse(
        Guid ProductCategoryId,
        string ProductCategoryCd,
        string CategoryAbbreviation,
        string ProductCategoryName,
        int DisplayOrder,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        Guid UpdatedBy,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    [HttpGet]
    [RequirePermission("product-categories.view")]
    public async Task<IActionResult> List(
        [FromQuery] string? productCategoryCd,
        [FromQuery] string? productCategoryName,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);

        var total = await _repo.CountAsync(productCategoryCd, productCategoryName, isActive, ct);
        var items = await _repo.ListAsync(productCategoryCd, productCategoryName, isActive, skip, pageSize, ct);

        return Ok(new { total, page, pageSize, items = items.Select(Map) });
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("product-categories.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    [RequirePermission("product-categories.create")]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _service.CreateAsync(req.ProductCategoryCd, req.CategoryAbbreviation, req.ProductCategoryName, req.IsActive, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.ProductCategoryId }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("product-categories.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateProductCategoryRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _service.UpdateAsync(id, req.ProductCategoryCd, req.CategoryAbbreviation, req.ProductCategoryName, userId, ct);
        return Ok(Map(entity));
    }

    public sealed record ChangeActivationRequest(bool IsActive);

    [HttpPut("{id:guid}/activation")]
    [RequirePermission("product-categories.delete")]
    public async Task<IActionResult> ChangeActivation(Guid id, [FromBody] ChangeActivationRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var entity = await _service.ChangeActivationAsync(id, req.IsActive, userId, ct);
        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    [RequirePermission("product-categories.edit")]
    public async Task<IActionResult> Reorder([FromBody] ReorderProductCategoriesRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _reorderValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entities = await _service.ReorderAsync(req.ProductCategoryIds, userId, ct);
        return Ok(entities.Select(Map));
    }

    private static ProductCategoryResponse Map(ProductCategory x)
        => new(
            x.ProductCategoryId,
            x.ProductCategoryCd,
            x.CategoryAbbreviation,
            x.ProductCategoryName,
            x.DisplayOrder,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}
