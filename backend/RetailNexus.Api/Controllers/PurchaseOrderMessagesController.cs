using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailNexus.Api.Authorization;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Api.Controllers;

[Route("api/purchase-orders/{purchaseOrderId:guid}/messages")]
[Authorize]
public sealed class PurchaseOrderMessagesController : BaseController
{
    private readonly IPurchaseOrderMessageService _service;
    private readonly IValidator<SendMessageRequest> _validator;

    public PurchaseOrderMessagesController(
        IPurchaseOrderMessageService service,
        IValidator<SendMessageRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    // ── Request / Response records ──

    public sealed record SendMessageRequest(string Body);

    public sealed record PurchaseOrderMessageResponse(
        Guid PurchaseOrderMessageId,
        Guid PurchaseOrderId,
        Guid SentBy,
        string SenderName,
        string Body,
        DateTimeOffset CreatedAt);

    // ── Endpoints ──

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> List(Guid purchaseOrderId, CancellationToken ct)
    {
        var messages = await _service.GetMessagesAsync(purchaseOrderId, ct);
        return Ok(messages.Select(MapResponse));
    }

    [HttpPost]
    [RequirePermission("purchases.edit")]
    public async Task<IActionResult> Send(Guid purchaseOrderId, [FromBody] SendMessageRequest req, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var actorUserId))
            return Unauthorized();

        var validation = await _validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var message = await _service.SendMessageAsync(purchaseOrderId, actorUserId, req.Body, ct);
        return Created($"api/purchase-orders/{purchaseOrderId}/messages", MapResponse(message));
    }

    // ── Helpers ──

    private static PurchaseOrderMessageResponse MapResponse(PurchaseOrderMessage x)
        => new(
            x.PurchaseOrderMessageId,
            x.PurchaseOrderId,
            x.SentBy,
            x.Sender?.UserName ?? "",
            x.Body,
            x.CreatedAt);
}
