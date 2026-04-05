using FluentAssertions;
using Moq;
using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Application.Services;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _repoMock = new();
    private readonly SupplierService _service;

    public SupplierServiceTests()
    {
        _service = new SupplierService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateCode_AndCreateEntity()
    {
        var actorId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetMaxSupplierCodeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("00001");

        var result = await _service.CreateAsync("テスト仕入先", "03-1234-5678", "test@example.com", true, actorId, CancellationToken.None);

        result.SupplierCode.Should().Be("00002");
        result.SupplierName.Should().Be("テスト仕入先");
        result.PhoneNumber.Should().Be("03-1234-5678");
        result.Email.Should().Be("test@example.com");
        result.IsActive.Should().BeTrue();

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallEntityUpdate()
    {
        var actorId = Guid.NewGuid();
        var entity = new Supplier("00001", "仕入先A", "03-1111-2222", "a@example.com", true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.UpdateAsync(entity.SupplierId, "仕入先B", "03-3333-4444", "b@example.com", actorId, CancellationToken.None);

        result.SupplierName.Should().Be("仕入先B");
        result.PhoneNumber.Should().Be("03-3333-4444");
        result.Email.Should().Be("b@example.com");
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var act = () => _service.UpdateAsync(id, "仕入先B", null, null, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangeActivationAsync_ShouldCallSetActivation()
    {
        var actorId = Guid.NewGuid();
        var entity = new Supplier("00001", "仕入先A", null, null, true, actorId);

        _repoMock.Setup(r => r.GetByIdAsync(entity.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.ChangeActivationAsync(entity.SupplierId, false, actorId, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
