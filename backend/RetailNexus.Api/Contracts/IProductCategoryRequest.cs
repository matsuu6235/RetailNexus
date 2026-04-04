namespace RetailNexus.Api.Contracts;

public interface IProductCategoryRequest
{
    string ProductCategoryCd { get; }
    string CategoryAbbreviation { get; }
    string ProductCategoryName { get; }
}
