using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISupplierRepository _repo;

    public SuppliersController(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public sealed record CreateSupplierRequest(
        string SupplierCode,
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive = true);
    
    public sealed record UpdateSupplierRequest(
        string SupplierCode,
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive = true);

    public sealed record SupplierNewResponse(
        string SupplierCode,
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive);

    public sealed record SupplierResponse(
        Guid SupplierId,
        string SupplierCode,
        string SupplierName,
        string? PhoneNumber,
        string? Email,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        Guid CreatedBy,
        Guid UpdatedBy);

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? supplierCode,
        [FromQuery] string? supplierName,
        [FromQuery] string? phoneNumber,
        [FromQuery] string? email,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var skip = (page - 1) * pageSize;
        var total = await _repo.CountAsync(supplierCode, supplierName, phoneNumber, email, isActive, ct);
        var items = await _repo.ListAsync(supplierCode, supplierName, phoneNumber, email, isActive, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(Map)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct);
        return supplier is null ? NotFound() : Ok(Map(supplier));
    }

    [HttpGet("new")]
    public IActionResult New()
        => Ok(new SupplierNewResponse(string.Empty, string.Empty, null, null, true));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest req, CancellationToken ct)
    {
        var code = req.SupplierCode.Trim();
        var name = req.SupplierName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("SupplierCode and SupplierName are required.");
        }

        var existing = await _repo.GetBySupplierCodeAsync(code, ct);
        if (existing is not null)
        {
            return Conflict("SupplierCode already exists.");
        }

        if (!TryGetCurrentUserId(out var actorUserId))
        {
            return Unauthorized();
        }

        var supplier = new Supplier(code, name, req.PhoneNumber, req.Email, req.IsActive, actorUserId);

        await _repo.AddAsync(supplier, ct);
        await _repo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId }, Map(supplier));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest req, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct);
        if (supplier is null)
        {
            return NotFound();
        }

        var code = req.SupplierCode.Trim();
        var name = req.SupplierName.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("SupplierCode and SupplierName are required.");
        }

        var duplicate = await _repo.GetBySupplierCodeAsync(code, ct);
        if (duplicate is not null && duplicate.SupplierId != id)
        {
            return Conflict("SupplierCode already exists.");
        }

        if (!TryGetCurrentUserId(out var actorUserId))
        {
            return Unauthorized();
        }

        supplier.Update(code, name, req.PhoneNumber, req.Email, req.IsActive, actorUserId);
        await _repo.SaveChangesAsync(ct);

        return Ok(Map(supplier));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out userId);
    }

    private static SupplierResponse Map(Supplier x)
        => new(
            x.SupplierId,
            x.SupplierCode,
            x.SupplierName,
            x.PhoneNumber,
            x.Email,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt,
            x.CreatedBy,
            x.UpdatedBy);
}
