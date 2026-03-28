namespace RetailNexus.Domain.Entities;

public class AuditLog
{
    public Guid AuditLogId { get; init; }
    public Guid? UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    private AuditLog() { }

    public AuditLog(
        Guid? userId,
        string userName,
        string action,
        string entityName,
        string entityId,
        string? oldValues,
        string? newValues)
    {
        UserId = userId;
        UserName = userName;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
    }
}
