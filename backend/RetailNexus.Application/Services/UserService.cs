using RetailNexus.Application.Exceptions;
using RetailNexus.Application.Interfaces;
using RetailNexus.Application.Interfaces.Services;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepo, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> CreateAsync(string loginId, string userName, string? email, string password, bool isActive, List<Guid> roleIds, Guid actorId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(loginId))
            throw new BusinessRuleException("LoginId", "ログインIDは必須です。");
        if (string.IsNullOrWhiteSpace(userName))
            throw new BusinessRuleException("UserName", "ユーザー名は必須です。");
        if (string.IsNullOrWhiteSpace(password))
            throw new BusinessRuleException("Password", "パスワードは必須です。");
        if (password.Length < 8)
            throw new BusinessRuleException("Password", "パスワードは8文字以上で入力してください。");

        var existing = await _userRepo.FindByLoginIdAsync(loginId.Trim(), ct);
        if (existing is not null)
            throw new DuplicateException("LoginId", "このログインIDは既に使用されています。");

        var hash = _passwordHasher.Hash(password);
        var user = new User(loginId.Trim(), userName.Trim(), email, hash, isActive, actorId, actorId);
        await _userRepo.AddAsync(user, ct);
        await _userRepo.SaveChangesAsync(ct);

        if (roleIds.Count > 0)
        {
            await _userRepo.ReplaceUserRolesAsync(user.UserId, roleIds, ct);
            await _userRepo.SaveChangesAsync(ct);
        }

        var created = await _userRepo.FindByIdWithRolesAsync(user.UserId, ct);
        return created!;
    }

    public async Task<User> UpdateAsync(Guid id, string loginId, string userName, string? email, List<Guid> roleIds, Guid actorId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(loginId))
            throw new BusinessRuleException("LoginId", "ログインIDは必須です。");
        if (string.IsNullOrWhiteSpace(userName))
            throw new BusinessRuleException("UserName", "ユーザー名は必須です。");

        var user = await _userRepo.FindByIdWithRolesAsync(id, ct)
            ?? throw new EntityNotFoundException("User", id);

        var duplicate = await _userRepo.FindByLoginIdAsync(loginId.Trim(), ct);
        if (duplicate is not null && duplicate.UserId != id)
            throw new DuplicateException("LoginId", "このログインIDは既に使用されています。");

        user.UpdateProfile(loginId.Trim(), userName.Trim(), email, actorId);

        await _userRepo.ReplaceUserRolesAsync(id, roleIds, ct);
        await _userRepo.SaveChangesAsync(ct);

        var updated = await _userRepo.FindByIdWithRolesAsync(id, ct);
        return updated!;
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword, Guid actorId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new BusinessRuleException("NewPassword", "パスワードは必須です。");
        if (newPassword.Length < 8)
            throw new BusinessRuleException("NewPassword", "パスワードは8文字以上で入力してください。");

        var user = await _userRepo.FindByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("User", id);

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userRepo.SaveChangesAsync(ct);
    }

    public async Task ChangeActivationAsync(Guid id, bool isActive, Guid actorId, CancellationToken ct)
    {
        var user = await _userRepo.FindByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("User", id);

        user.IsActive = isActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = actorId;
        await _userRepo.SaveChangesAsync(ct);
    }
}
