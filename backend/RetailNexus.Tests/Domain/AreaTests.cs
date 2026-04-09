using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class AreaTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var area = new Area("01", "関東", 1, true, _actorUserId);

        area.AreaCode.Should().Be("01");
        area.AreaName.Should().Be("関東");
        area.DisplayOrder.Should().Be(1);
        area.IsActive.Should().BeTrue();
        area.CreatedBy.Should().Be(_actorUserId);
        area.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var area = new Area("01", "関東", 1, true, _actorUserId);
        var updater = Guid.NewGuid();

        area.Update("02", "関西", updater);

        area.AreaCode.Should().Be("02");
        area.AreaName.Should().Be("関西");
        area.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetActivation_ShouldChangeIsActiveAndUpdateAudit()
    {
        var area = new Area("01", "関東", 1, true, _actorUserId);
        var updater = Guid.NewGuid();
        var before = area.UpdatedAt;

        area.SetActivation(false, updater);

        area.IsActive.Should().BeFalse();
        area.UpdatedBy.Should().Be(updater);
        area.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldUpdateTimestamp()
    {
        var area = new Area("01", "関東", 1, true, _actorUserId);
        var before = area.UpdatedAt;

        area.Update("02", "関西", _actorUserId);

        area.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void SetDisplayOrder_ShouldUpdateOrderAndAudit()
    {
        var area = new Area("01", "関東", 1, true, _actorUserId);
        var updater = Guid.NewGuid();

        area.SetDisplayOrder(5, updater);

        area.DisplayOrder.Should().Be(5);
        area.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void Constructor_ShouldNotPreserveDisplayOrderOnUpdate()
    {
        var area = new Area("01", "関東", 3, true, _actorUserId);

        area.Update("01", "関東エリア", _actorUserId);

        area.DisplayOrder.Should().Be(3);
    }
}
