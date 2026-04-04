namespace RetailNexus.Api.Contracts;

public interface ISupplierRequest
{
    string SupplierName { get; }
    string? PhoneNumber { get; }
    string? Email { get; }
}
