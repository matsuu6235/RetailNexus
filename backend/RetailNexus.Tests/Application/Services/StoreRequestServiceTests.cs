using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Features.StoreRequests;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;
using RetailNexus.Domain.Enums;

namespace RetailNexus.Tests.Application.Services;

public class StoreRequestServiceTests
{
    private readonly Mock<IStoreRequestRepository> _repoMock = new();
    private readonly StoreRequestService _service;

    public StoreRequestServiceTests()
    {
        _service = new StoreRequestService(_repoMock.Object);
    }

    private static StoreRequest CreateDraftRequest(Guid actorId)
    {
        return new StoreRequest("SR-000001", Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7), "テスト備考", actorId);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateRequestNumber_CreateWithDetails()
    {
        var actorId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var details = new List<CreateStoreRequestDetailParam>
        {
            new(productId, 5)
        };

        _repoMock.Setup(r => r.GetMaxRequestNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var request = new StoreRequest("SR-000001", Guid.NewGuid(), Guid.NewGuid(),
                    DateTimeOffset.UtcNow, null, null, actorId);
                return request;
            });

        var result = await _service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7), "テスト備考", details, actorId, CancellationToken.None);

        result.RequestNumber.Should().Be("SR-000001");
        _repoMock.Verify(r => r.AddAsync(It.Is<StoreRequest>(sr => sr.RequestNumber == "SR-000001"), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldCallDomainMethod()
    {
        var actorId = Guid.NewGuid();
        var request = CreateDraftRequest(actorId);

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(request.StoreRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _service.SubmitForApprovalAsync(request.StoreRequestId, actorId, CancellationToken.None);

        result.Status.Should().Be(StoreRequestStatus.AwaitingApproval);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldCallDomainMethod()
    {
        var actorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = CreateDraftRequest(actorId);
        request.SubmitForApproval(actorId);

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(request.StoreRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _service.ApproveAsync(request.StoreRequestId, approverId, CancellationToken.None);

        result.Status.Should().Be(StoreRequestStatus.Approved);
        result.ApprovedBy.Should().Be(approverId);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ShouldCallDomainMethod()
    {
        var actorId = Guid.NewGuid();
        var request = CreateDraftRequest(actorId);
        request.SubmitForApproval(actorId);

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(request.StoreRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _service.RejectAsync(request.StoreRequestId, actorId, CancellationToken.None);

        result.Status.Should().Be(StoreRequestStatus.Draft);
        result.ApprovedBy.Should().BeNull();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var request = CreateDraftRequest(actorId);

        _repoMock.Setup(r => r.GetByIdAsync(request.StoreRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _service.ChangeActivationAsync(request.StoreRequestId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
