using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence;

public class RetailNexusDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    private static readonly HashSet<string> ExcludedProperties = new() { "PasswordHash", "LastLoginAt", "UpdatedAt", "UpdatedBy" };

    public RetailNexusDbContext(
        DbContextOptions<RetailNexusDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreType> StoreTypes => Set<StoreType>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailNexusDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var (userId, userName) = GetCurrentUser();
        var auditEntries = CollectAuditEntries(userId, userName);

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            foreach (var entry in auditEntries)
            {
                var entityId = entry.Entry.Property(entry.PkPropertyName).CurrentValue?.ToString() ?? "";
                AuditLogs.Add(new AuditLog(
                    entry.UserId,
                    entry.UserName,
                    entry.Action,
                    entry.EntityName,
                    entityId,
                    entry.OldValues.Count > 0 ? JsonSerializer.Serialize(entry.OldValues) : null,
                    entry.NewValues.Count > 0 ? JsonSerializer.Serialize(entry.NewValues) : null));
            }

            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private (Guid? UserId, string UserName) GetCurrentUser()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
            return (null, "System");

        var subClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? httpContext.User.FindFirst("sub")?.Value;

        var nameClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                        ?? httpContext.User.FindFirst(ClaimTypes.Name)?.Value
                        ?? "System";

        Guid.TryParse(subClaim, out var userId);
        return (userId == Guid.Empty ? null : userId, nameClaim);
    }

    private List<AuditEntry> CollectAuditEntries(Guid? userId, string userName)
    {
        var entries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog)
                continue;

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var entityType = entry.Entity.GetType();
            var pkProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
            if (pkProperty is null)
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => ""
            };

            var auditEntry = new AuditEntry
            {
                Entry = entry,
                EntityName = entityType.Name,
                PkPropertyName = pkProperty.Name,
                Action = action,
                UserId = userId,
                UserName = userName
            };

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsShadowProperty())
                    continue;

                if (ExcludedProperties.Contains(propertyName))
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Modified when property.IsModified:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;
                }
            }

            if (auditEntry.OldValues.Count > 0 || auditEntry.NewValues.Count > 0)
                entries.Add(auditEntry);
        }

        return entries;
    }

    private class AuditEntry
    {
        public EntityEntry Entry { get; init; } = null!;
        public string EntityName { get; init; } = string.Empty;
        public string PkPropertyName { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public Guid? UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
    }
}
