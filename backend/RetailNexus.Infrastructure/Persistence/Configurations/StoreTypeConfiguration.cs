using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class StoreTypeConfiguration : IEntityTypeConfiguration<StoreType>
{
    public void Configure(EntityTypeBuilder<StoreType> b)
    {
        b.ToTable("store_types");

        b.HasKey(x => x.StoreTypeId);

        b.Property(x => x.StoreTypeId)
            .HasColumnName("store_type_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.StoreTypeCd)
            .HasColumnName("store_type_cd")
            .HasMaxLength(2)
            .IsRequired();
        b.HasIndex(x => x.StoreTypeCd).IsUnique();

        b.Property(x => x.StoreTypeName)
            .HasColumnName("store_type_name")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();
        b.HasIndex(x => x.IsActive);

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