using FluentAssertions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Domain;

public class SupplierTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var supplier = new Supplier("00001", "テスト仕入先", "03-1234-5678", "test@example.com", true, _actorUserId);

        supplier.SupplierCode.Should().Be("00001");
        supplier.SupplierName.Should().Be("テスト仕入先");
        supplier.PhoneNumber.Should().Be("03-1234-5678");
        supplier.Email.Should().Be("test@example.com");
        supplier.IsActive.Should().BeTrue();
        supplier.CreatedBy.Should().Be(_actorUserId);
        supplier.UpdatedBy.Should().Be(_actorUserId);
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmptyOptionalFieldsToNull()
    {
        var supplier = new Supplier("00001", "テスト仕入先", "", "", true, _actorUserId);

        supplier.PhoneNumber.Should().BeNull();
        supplier.Email.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldNormalizeWhitespaceOnlyFieldsToNull()
    {
        var supplier = new Supplier("00001", "テスト仕入先", "   ", "   ", true, _actorUserId);

        supplier.PhoneNumber.Should().BeNull();
        supplier.Email.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldAllowNullOptionalFields()
    {
        var supplier = new Supplier("00001", "テスト仕入先", null, null, true, _actorUserId);

        supplier.PhoneNumber.Should().BeNull();
        supplier.Email.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var supplier = new Supplier("00001", "テスト仕入先", null, null, true, _actorUserId);
        var updater = Guid.NewGuid();

        supplier.Update("更新仕入先", "090-1111-2222", "new@example.com", updater);

        supplier.SupplierCode.Should().Be("00001");
        supplier.SupplierName.Should().Be("更新仕入先");
        supplier.PhoneNumber.Should().Be("090-1111-2222");
        supplier.Email.Should().Be("new@example.com");
        supplier.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetActivation_ShouldChangeIsActiveAndUpdateAudit()
    {
        var supplier = new Supplier("00001", "テスト仕入先", null, null, true, _actorUserId);
        var updater = Guid.NewGuid();
        var before = supplier.UpdatedAt;

        supplier.SetActivation(false, updater);

        supplier.IsActive.Should().BeFalse();
        supplier.UpdatedBy.Should().Be(updater);
        supplier.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldNormalizeEmptyOptionalFieldsToNull()
    {
        var supplier = new Supplier("00001", "テスト仕入先", "03-1234-5678", "test@example.com", true, _actorUserId);

        supplier.Update("テスト仕入先", "", "", _actorUserId);

        supplier.PhoneNumber.Should().BeNull();
        supplier.Email.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateTimestamp()
    {
        var supplier = new Supplier("00001", "テスト仕入先", null, null, true, _actorUserId);
        var before = supplier.UpdatedAt;

        supplier.Update("テスト仕入先改", null, null, _actorUserId);

        supplier.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldNotChangeCreatedByOrSupplierCode()
    {
        var supplier = new Supplier("00001", "テスト仕入先", null, null, true, _actorUserId);
        var updater = Guid.NewGuid();

        supplier.Update("更新仕入先", null, null, updater);

        supplier.SupplierCode.Should().Be("00001");
        supplier.CreatedBy.Should().Be(_actorUserId);
        supplier.UpdatedBy.Should().Be(updater);
    }
}
