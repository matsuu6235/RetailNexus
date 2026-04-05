namespace RetailNexus.Application.Features.PurchaseOrders;

public sealed record CreatePurchaseOrderDetailParam(Guid ProductId, int Quantity, decimal UnitPrice);
public sealed record UpdatePurchaseOrderDetailParam(Guid? DetailId, Guid ProductId, int Quantity, decimal UnitPrice);
