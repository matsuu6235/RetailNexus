using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoryRepository _repo;
    private readonly IValidator<CreateProductCategoryRequest> _createValidator;
    private readonly IValidator<UpdateProductCategoryRequest> _updateValidator;
    private readonly IValidator<ReorderProductCategoriesRequest> _reorderValidator;

    public ProductCategoriesController(
        IProductCategoryRepository repo,
        IValidator<CreateProductCategoryRequest> createValidator,
        IValidator<UpdateProductCategoryRequest> updateValidator,
        IValidator<ReorderProductCategoriesRequest> reorderValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    public sealed record CreateProductCategoryRequest(string ProductCategoryCd, string ProductCategoryName, bool IsActive = true);
    public sealed record UpdateProductCategoryRequest(string ProductCategoryCd, string ProductCategoryName, bool IsActive = true);
    public sealed record ReorderProductCategoriesRequest(IReadOnlyList<Guid> ProductCategoryIds);
    public sealed record ProductCategoryResponse(
        Guid ProductCategoryId,
        string ProductCategoryCd,
        string ProductCategoryName,
        int DisplayOrder,
        bool IsActive,
        DateTimeOffset UpdatedAt,
        Guid UpdatedBy,
        DateTimeOffset CreatedAt,
        Guid CreatedBy);

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? productCategoryCd,
        [FromQuery] string? productCategoryName,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var skip = (page - 1) * pageSize;

        var total = await _repo.CountAsync(productCategoryCd, productCategoryName, isActive, ct);
        var items = await _repo.ListAsync(productCategoryCd, productCategoryName, isActive, skip, pageSize, ct);

        return Ok(new { total, page, pageSize, items = items.Select(Map) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var code = req.ProductCategoryCd.Trim();
        var name = req.ProductCategoryName.Trim();
        var nextDisplayOrder = await _repo.GetNextDisplayOrderAsync(ct);
        var entity = new ProductCategory(code, name, nextDisplayOrder, req.IsActive, userId);

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.ProductCategoryId }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var ctx = new ValidationContext<UpdateProductCategoryRequest>(req);
        ctx.RootContextData["EntityId"] = id;
        var validation = await _updateValidator.ValidateAsync(ctx, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        entity.Update(req.ProductCategoryCd.Trim(), req.ProductCategoryName.Trim(), req.IsActive, userId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(entity));
    }

    [HttpPut("display-order")]
    public async Task<IActionResult> Reorder([FromBody] ReorderProductCategoriesRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var validation = await _reorderValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var distinctIds = req.ProductCategoryIds.Distinct().ToArray();
        var entities = await _repo.GetByIdsAsync(distinctIds, ct);

        var orderMap = req.ProductCategoryIds
            .Select((id, index) => new { id, displayOrder = index + 1 })
            .ToDictionary(x => x.id, x => x.displayOrder);

        foreach (var entity in entities)
        {
            entity.SetDisplayOrder(orderMap[entity.ProductCategoryId], userId);
        }

        await _repo.SaveChangesAsync(ct);

        return Ok(entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ProductCategoryCd)
            .Select(Map));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out userId);
    }

    private static ProductCategoryResponse Map(ProductCategory x)
        => new(
            x.ProductCategoryId,
            x.ProductCategoryCd,
            x.ProductCategoryName,
            x.DisplayOrder,
            x.IsActive,
            x.UpdatedAt,
            x.UpdatedBy,
            x.CreatedAt,
            x.CreatedBy);
}
