namespace RetailNexus.Domain.Entities;

public class StoreType
{
    public Guid StoreTypeId { get; private set; } = Guid.NewGuid();
    public string StoreTypeCd { get; private set; } = string.Empty;
    public string StoreTypeName { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    private StoreType() { }

    public StoreType(string storeTypeCd, string storeTypeName, int displayOrder, bool isActive, Guid actorUserId)
    {
        StoreTypeCd = storeTypeCd.Trim();
        StoreTypeName = storeTypeName.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(string storeTypeCd, string storeTypeName, bool isActive, Guid actorUserId)
    {
        StoreTypeCd = storeTypeCd.Trim();
        StoreTypeName = storeTypeName.Trim();
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