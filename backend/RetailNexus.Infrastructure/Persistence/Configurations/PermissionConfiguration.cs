using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("permissions");

        b.HasKey(x => x.PermissionId);

        b.Property(x => x.PermissionId)
            .HasColumnName("permission_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.PermissionCode)
            .HasColumnName("permission_code")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.PermissionName)
            .HasColumnName("permission_name")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired();

        b.HasIndex(x => x.PermissionCode)
            .IsUnique();
    }
}
