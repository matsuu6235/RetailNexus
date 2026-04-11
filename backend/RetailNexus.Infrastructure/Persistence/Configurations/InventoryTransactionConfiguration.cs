using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> b)
    {
        b.ToTable("inventory_transactions");

        b.HasKey(x => x.InventoryTransactionId);

        b.Property(x => x.InventoryTransactionId)
            .HasColumnName("inventory_transaction_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.StoreId)
            .HasColumnName("store_id")
            .IsRequired();

        b.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        b.Property(x => x.TransactionType)
            .HasColumnName("transaction_type")
            .IsRequired();

        b.Property(x => x.QuantityChange)
            .HasColumnName("quantity_change")
            .HasPrecision(12, 2)
            .IsRequired();

        b.Property(x => x.QuantityAfter)
            .HasColumnName("quantity_after")
            .HasPrecision(12, 2)
            .IsRequired();

        b.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        b.Property(x => x.ReferenceNumber)
            .HasColumnName("reference_number")
            .HasMaxLength(50);

        b.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        b.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.StoreId, x.ProductId, x.OccurredAt });
    }
}
