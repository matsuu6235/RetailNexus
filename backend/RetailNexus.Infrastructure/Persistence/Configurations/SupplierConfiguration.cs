using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("suppliers");

        b.HasKey(x => x.SupplierId);

        b.Property(x => x.SupplierId)
            .HasColumnName("supplier_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.SupplierCode)
            .HasColumnName("supplier_code")
            .HasMaxLength(30)
            .IsRequired();
        b.HasIndex(x => x.SupplierCode)
            .IsUnique();

        b.Property(x => x.SupplierName)
            .HasColumnName("supplier_name")
            .HasMaxLength(100)
            .IsRequired();
        b.HasIndex(x => x.SupplierName);

        b.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        b.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();
        b.HasIndex(x => x.IsActive);

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        b.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .IsRequired();
    }
}
