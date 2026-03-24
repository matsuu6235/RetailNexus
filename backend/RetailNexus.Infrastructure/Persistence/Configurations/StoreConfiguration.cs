using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> b)
    {
        b.ToTable("stores");

        b.HasKey(x => x.StoreId);

        b.Property(x => x.StoreId)
            .HasColumnName("store_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.StoreCd)
            .HasColumnName("store_cd")
            .HasMaxLength(6)
            .IsRequired();

        b.Property(x => x.StoreName)
            .HasColumnName("store_name")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.AreaId)
            .HasColumnName("area_id")
            .IsRequired();

        b.Property(x => x.StoreTypeId)
            .HasColumnName("store_type_id")
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

        b.HasOne(x => x.Area)
            .WithMany()
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.StoreType)
            .WithMany()
            .HasForeignKey(x => x.StoreTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}