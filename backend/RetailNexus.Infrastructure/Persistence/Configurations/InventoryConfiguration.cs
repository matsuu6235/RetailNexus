using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> b)
    {
        b.ToTable("inventories");

        b.HasKey(x => x.InventoryId);

        b.Property(x => x.InventoryId)
            .HasColumnName("inventory_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        b.Property(x => x.StoreId)
            .HasColumnName("store_id")
            .IsRequired();

        b.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(12, 2)
            .IsRequired();

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

        b.HasIndex(x => new { x.ProductId, x.StoreId }).IsUnique();

        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
