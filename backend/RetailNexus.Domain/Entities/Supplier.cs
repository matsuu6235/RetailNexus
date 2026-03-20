namespace RetailNexus.Domain.Entities;

public class Supplier
{
    public Guid SupplierId { get; private set; } = Guid.NewGuid();
    public string SupplierCode { get; private set; } = string.Empty;
    public string SupplierName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    private Supplier()
    {
    }

    public Supplier(
        string supplierCode,
        string supplierName,
        string? phoneNumber,
        string? email,
        bool isActive,
        Guid actorUserId)
    {
        SetBasic(supplierCode, supplierName, phoneNumber, email);
        IsActive = isActive;
        CreatedBy = actorUserId;
        UpdatedBy = actorUserId;
    }

    public void Update(
        string supplierCode,
        string supplierName,
        string? phoneNumber,
        string? email,
        bool isActive,
        Guid actorUserId)
    {
        SetBasic(supplierCode, supplierName, phoneNumber, email);
        IsActive = isActive;
        UpdatedBy = actorUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void SetBasic(string supplierCode, string supplierName, string? phoneNumber, string? email)
    {
        SupplierCode = supplierCode.Trim();
        SupplierName = supplierName.Trim();
        PhoneNumber = NormalizeOptional(phoneNumber);
        Email = NormalizeOptional(email);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
