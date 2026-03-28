using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class StoreTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _areaId = Guid.NewGuid();
    private readonly Guid _storeTypeId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var store = new Store("000001", "渋谷店", _areaId, _storeTypeId, true, _actorUserId);

        store.StoreCd.Should().Be("000001");
        store.StoreName.Should().Be("渋谷店");
        store.AreaId.Should().Be(_areaId);
        store.StoreTypeId.Should().Be(_storeTypeId);
        store.IsActive.Should().BeTrue();
        store.CreatedBy.Should().Be(_actorUserId);
        store.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        var store = new Store("  000001  ", "  渋谷店  ", _areaId, _storeTypeId, true, _actorUserId);

        store.StoreCd.Should().Be("000001");
        store.StoreName.Should().Be("渋谷店");
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var store = new Store("000001", "渋谷店", _areaId, _storeTypeId, true, _actorUserId);
        var newAreaId = Guid.NewGuid();
        var newStoreTypeId = Guid.NewGuid();
        var updater = Guid.NewGuid();

        store.Update("新宿店", newAreaId, newStoreTypeId, updater);

        store.StoreCd.Should().Be("000001");
        store.StoreName.Should().Be("新宿店");
        store.AreaId.Should().Be(newAreaId);
        store.StoreTypeId.Should().Be(newStoreTypeId);
        store.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetActivation_ShouldChangeIsActiveAndUpdateAudit()
    {
        var store = new Store("000001", "渋谷店", _areaId, _storeTypeId, true, _actorUserId);
        var updater = Guid.NewGuid();
        var before = store.UpdatedAt;

        store.SetActivation(false, updater);

        store.IsActive.Should().BeFalse();
        store.UpdatedBy.Should().Be(updater);
        store.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldTrimWhitespace()
    {
        var store = new Store("000001", "渋谷店", _areaId, _storeTypeId, true, _actorUserId);

        store.Update("  新宿店  ", _areaId, _storeTypeId, _actorUserId);

        store.StoreName.Should().Be("新宿店");
    }

    [Fact]
    public void Update_ShouldUpdateTimestamp()
    {
        var store = new Store("000001", "渋谷店", _areaId, _storeTypeId, true, _actorUserId);
        var before = store.UpdatedAt;

        store.Update("渋谷店改", _areaId, _storeTypeId, _actorUserId);

        store.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldNotChangeCreatedByOrStoreCd()
    {
        var store = new Store("000001", "渋谷店", _areaId, _storeTypeId, true, _actorUserId);
        var updater = Guid.NewGuid();

        store.Update("渋谷店", _areaId, _storeTypeId, updater);

        store.StoreCd.Should().Be("000001");
        store.CreatedBy.Should().Be(_actorUserId);
        store.UpdatedBy.Should().Be(updater);
    }
}
