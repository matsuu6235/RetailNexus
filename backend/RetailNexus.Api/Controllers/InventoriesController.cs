using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[Route("api/inventories")]
[Authorize]
public sealed class InventoriesController : BaseController
{
    private readonly IInventoryRepository _repo;

    public InventoriesController(IInventoryRepository repo)
    {
        _repo = repo;
    }

    // ── Response records ──

    public sealed record InventoryListResponse(
        Guid InventoryId,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        string ProductCategoryCode,
        Guid StoreId,
        string StoreCode,
        string StoreName,
        string AreaName,
        decimal Quantity,
        DateTimeOffset UpdatedAt);

    // ── Endpoints ──

    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? areaId,
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? productCategoryId,
        [FromQuery] string? productCode,
        [FromQuery] Guid? supplierId,
        [FromQuery] string? stockStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        (var skip, page, pageSize) = NormalizePagination(page, pageSize);
        var total = await _repo.CountAsync(areaId, storeId, productCategoryId, productCode, supplierId, stockStatus, ct);
        var items = await _repo.ListAsync(areaId, storeId, productCategoryId, productCode, supplierId, stockStatus, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(MapList)
        });
    }

    // ── Helpers ──

    private static InventoryListResponse MapList(Inventory x)
        => new(
            x.InventoryId,
            x.ProductId,
            x.Product?.ProductCode ?? "",
            x.Product?.ProductName ?? "",
            x.Product?.ProductCategoryCode ?? "",
            x.StoreId,
            x.Store?.StoreCode ?? "",
            x.Store?.StoreName ?? "",
            x.Store?.Area?.AreaName ?? "",
            x.Quantity,
            x.UpdatedAt);
}
