namespace RetailNexus.Domain.Entities;

public class Store
{
    public Guid StoreId { get; private set; } = Guid.NewGuid();
    public string StoreCd { get; private set; } = string.Empty;
    public string StoreName { get; private set; } = string.Empty;
    public Guid AreaId { get; private set; }
    public Guid StoreTypeId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    public Area? Area { get; private set; }
    public StoreType? StoreType { get; private set; }

    private Store() { }

    public Store(string storeCd, string storeName, Guid areaId, Guid storeTypeId, bool isActive, Guid actorUserId)
    {
        StoreCd = storeCd.Trim();
        StoreName = storeName.Trim();
        AreaId = areaId;
        StoreTypeId = storeTypeId;
        IsActive = isActive;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(string storeCd, string storeName, Guid areaId, Guid storeTypeId, bool isActive, Guid actorUserId)
    {
        StoreCd = storeCd.Trim();
        StoreName = storeName.Trim();
        AreaId = areaId;
        StoreTypeId = storeTypeId;
        IsActive = isActive;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}