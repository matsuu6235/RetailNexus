using Microsoft.EntityFrameworkCore;
using RetailNexus.Application.Interfaces;
using RetailNexus.Domain.Entities;
using RetailNexus.Infrastructure.Persistence;

namespace RetailNexus.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly RetailNexusDbContext _db;

    public UserRepository(RetailNexusDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByLoginIdAsync(string loginId, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.LoginId == loginId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}