using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs");

        b.HasKey(x => x.AuditLogId);

        b.Property(x => x.AuditLogId)
            .HasColumnName("audit_log_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.UserId)
            .HasColumnName("user_id");

        b.Property(x => x.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(10)
            .IsRequired();

        b.Property(x => x.EntityName)
            .HasColumnName("entity_name")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("text");

        b.Property(x => x.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("text");

        b.Property(x => x.Timestamp)
            .HasColumnName("timestamp")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.HasIndex(x => x.Timestamp)
            .IsDescending()
            .HasDatabaseName("ix_audit_logs_timestamp");

        b.HasIndex(x => new { x.EntityName, x.EntityId })
            .HasDatabaseName("ix_audit_logs_entity");
    }
}
