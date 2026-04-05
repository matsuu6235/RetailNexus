using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IProductService
{
    Task<Product> CreateAsync(string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid actorId, CancellationToken ct);
    Task<Product> UpdateAsync(Guid id, string janCode, string productName, decimal price, decimal cost, string productCategoryCode, Guid actorId, CancellationToken ct);
    Task<Product> ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
