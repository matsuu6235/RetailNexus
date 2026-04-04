namespace RetailNexus.Api.Contracts;

public interface IStoreRequest
{
    string StoreName { get; }
    Guid AreaId { get; }
    Guid StoreTypeId { get; }
}
