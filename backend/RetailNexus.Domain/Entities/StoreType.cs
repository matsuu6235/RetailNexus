namespace RetailNexus.Domain.Entities;

public class StoreType
{
    public Guid StoreTypeId { get; private set; } = Guid.NewGuid();
    public string StoreTypeCode { get; private set; } = string.Empty;
    public string StoreTypeName { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    private StoreType() { }

    public StoreType(string storeTypeCode, string storeTypeName, int displayOrder, bool isActive, Guid actorUserId)
    {
        StoreTypeCode = storeTypeCode;
        StoreTypeName = storeTypeName;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(string storeTypeCode, string storeTypeName, Guid actorUserId)
    {
        StoreTypeCode = storeTypeCode;
        StoreTypeName = storeTypeName;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActivation(bool isActive, Guid actorUserId)
    {
        IsActive = isActive;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDisplayOrder(int displayOrder, Guid actorUserId)
    {
        DisplayOrder = displayOrder;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}