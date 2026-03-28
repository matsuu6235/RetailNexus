using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles");

        b.HasKey(x => x.RoleId);

        b.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.RoleName)
            .HasColumnName("role_name")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(200);

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by");
    }
}
