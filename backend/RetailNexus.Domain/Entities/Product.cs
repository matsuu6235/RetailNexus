namespace RetailNexus.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ProductCode { get; private set; } = string.Empty;
    public string JanCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string ProductCategoryCode { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal Cost { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; private set; }

    private Product()
    {
    }

    public Product(string productCode, string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid? createdBy = null)
    {
        ProductCode = productCode;
        JanCode = janCode;
        ProductName = productName;
        Price = price;
        Cost = cost;
        ProductCategoryCode = productCategoryCode;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }

    public void Update(string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid? updatedBy = null)
    {
        JanCode = janCode;
        ProductName = productName;
        Price = price;
        Cost = cost;
        ProductCategoryCode = productCategoryCode;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActivation(bool isActive, Guid? updatedBy = null)
    {
        IsActive = isActive;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
