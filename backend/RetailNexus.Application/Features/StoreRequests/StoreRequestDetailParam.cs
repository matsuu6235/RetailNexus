namespace RetailNexus.Application.Features.StoreRequests;

public sealed record CreateStoreRequestDetailParam(Guid ProductId, int Quantity);
public sealed record UpdateStoreRequestDetailParam(Guid? DetailId, Guid ProductId, int Quantity);
