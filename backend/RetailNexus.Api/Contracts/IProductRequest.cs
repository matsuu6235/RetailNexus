namespace RetailNexus.Api.Contracts;

public interface IProductRequest
{
    string JanCode { get; }
    string ProductName { get; }
    decimal Price { get; }
    decimal Cost { get; }
    string ProductCategoryCode { get; }
}
