namespace RetailNexus.Domain.Entities;

public class Area
{
    public Guid AreaId { get; private set; } = Guid.NewGuid();
    public string AreaCd { get; private set; } = string.Empty;
    public string AreaName { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    private Area() { }

    public Area(string areaCd, string areaName, int displayOrder, bool isActive, Guid actorUserId)
    {
        AreaCd = areaCd.Trim();
        AreaName = areaName.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(string areaCd, string areaName, Guid actorUserId)
    {
        AreaCd = areaCd.Trim();
        AreaName = areaName.Trim();
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