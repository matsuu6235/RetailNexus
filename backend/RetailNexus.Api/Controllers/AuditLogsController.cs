using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _repo;

    public AuditLogsController(IAuditLogRepository repo)
    {
        _repo = repo;
    }

    public sealed record AuditLogResponse(
        Guid AuditLogId,
        Guid? UserId,
        string UserName,
        string Action,
        string EntityName,
        string EntityId,
        string? OldValues,
        string? NewValues,
        DateTimeOffset Timestamp);

    [HttpGet]
    [RequirePermission("auditlog.view")]
    public async Task<IActionResult> List(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? userName,
        [FromQuery] string? action,
        [FromQuery] string? entityName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var skip = (page - 1) * pageSize;

        var total = await _repo.CountAsync(from, to, userName, action, entityName, ct);
        var items = await _repo.ListAsync(from, to, userName, action, entityName, skip, pageSize, ct);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(Map)
        });
    }

    private static AuditLogResponse Map(AuditLog x) => new(
        x.AuditLogId,
        x.UserId,
        x.UserName,
        x.Action,
        x.EntityName,
        x.EntityId,
        x.OldValues,
        x.NewValues,
        x.Timestamp);
}
