namespace RetailNexus.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ProductCode { get; private set; } = string.Empty; // 商品コード
    public string JanCode { get; private set; } = string.Empty;// JAN
    public string ProductName { get; private set; } = string.Empty;
    public string ProductCategoryCode { get; private set; }　= string.Empty;
    public decimal Price { get; private set; } // 売価
    public decimal Cost { get; private set; }  // 原価
    public bool IsActive { get; private set; } = true;
    
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    
    private Product()
    {
    }

    public Product(string productCode, string janCode, string productName, decimal price, decimal cost, string productCategoryCode)
    {
        ProductCode = productCode;
        JanCode = janCode;
        ProductName = productName;
        Price = price;
        Cost = cost;
        ProductCategoryCode = productCategoryCode;
    }

    public void Update(string janCode, string productName, decimal price, decimal cost, string productCategoryCode, bool isActive)
    {
        JanCode = janCode;
        ProductName = productName;
        Price = price;
        Cost = cost;
        ProductCategoryCode = productCategoryCode;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}