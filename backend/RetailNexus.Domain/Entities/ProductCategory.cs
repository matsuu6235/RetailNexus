namespace RetailNexus.Domain.Entities;

public class ProductCategory
{
    public Guid ProductCategoryId { get; private set; } = Guid.NewGuid();
    public string ProductCategoryCd { get; private set; } = string.Empty;
    public string ProductCategoryName { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }

    private ProductCategory()
    {
    }

    public ProductCategory(string cd, string name, int displayOrder, bool isActive, Guid actorUserId)
    {
        ProductCategoryCd = cd.Trim();
        ProductCategoryName = name.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(string cd, string name, bool isActive, Guid actorUserId)
    {
        ProductCategoryCd = cd.Trim();
        ProductCategoryName = name.Trim();
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
