using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> b)
    {
        b.ToTable("areas");

        b.HasKey(x => x.AreaId);

        b.Property(x => x.AreaId)
            .HasColumnName("area_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.AreaCode)
            .HasColumnName("area_code")
            .HasMaxLength(2)
            .IsRequired();

        b.Property(x => x.AreaName)
            .HasColumnName("area_name")
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();
    }
}