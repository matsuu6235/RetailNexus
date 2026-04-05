using RetailNexus.Application.Features.StoreRequests;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Application.Interfaces.Services;

public interface IStoreRequestService
{
    Task<StoreRequest> CreateAsync(Guid fromStoreId, Guid toStoreId, DateTimeOffset requestDate,
        DateTimeOffset? desiredDeliveryDate, string? note,
        List<CreateStoreRequestDetailParam> details, Guid actorId, CancellationToken ct);

    Task<StoreRequest> UpdateAsync(Guid id, Guid fromStoreId, Guid toStoreId, DateTimeOffset requestDate,
        DateTimeOffset? desiredDeliveryDate, DateTimeOffset? expectedDeliveryDate, string? note,
        List<UpdateStoreRequestDetailParam> details, Guid actorId, CancellationToken ct);

    Task<StoreRequest> SubmitForApprovalAsync(Guid id, Guid actorId, CancellationToken ct);
    Task<StoreRequest> ApproveAsync(Guid id, Guid approverId, CancellationToken ct);
    Task<StoreRequest> RejectAsync(Guid id, Guid actorId, CancellationToken ct);
    Task<StoreRequest> ChangeStatusAsync(Guid id, StoreRequestStatus status, Guid actorId, CancellationToken ct);
    Task<StoreRequest> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
