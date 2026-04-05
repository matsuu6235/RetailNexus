using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Interfaces.Services;

public interface IUserService
{
    Task<User> CreateAsync(string loginId, string userName, string? email, string password, bool isActive, List<Guid> roleIds, Guid actorId, CancellationToken ct);
    Task<User> UpdateAsync(Guid id, string loginId, string userName, string? email, List<Guid> roleIds, Guid actorId, CancellationToken ct);
    Task ResetPasswordAsync(Guid id, string newPassword, Guid actorId, CancellationToken ct);
    Task ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct);
}
