using System.ComponentModel.DataAnnotations.Schema;

namespace RetailNexus.Domain.Entities;

public class User
{
    public Guid UserId { get; init; }
    public string LoginId { get;　private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string? Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty; // MVPでは平文っぽく扱うが将来はハッシュ必須
    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? UpdatedBy { get; set; }
    
    [NotMapped]
    public string Role => "Admin";

    private User()
    {
    }

    public User(
        string loginId,
        string userName,
        string? email,
        string passwordHash,
        bool isActive,
        Guid? createdBy,
        Guid? updatedBy)
    {
        LoginId = loginId.Trim();
        UserName = userName.Trim();
        Email = NormalizeNullable(email);
        PasswordHash = passwordHash;
        IsActive = isActive;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}