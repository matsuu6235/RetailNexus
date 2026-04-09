namespace RetailNexus.Api.Contracts;

public interface IProductCategoryRequest
{
    string ProductCategoryCode { get; }
    string CategoryAbbreviation { get; }
    string ProductCategoryName { get; }
}
