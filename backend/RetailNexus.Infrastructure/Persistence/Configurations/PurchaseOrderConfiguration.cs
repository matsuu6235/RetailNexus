using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("purchase_orders");

        b.HasKey(x => x.PurchaseOrderId);

        b.Property(x => x.PurchaseOrderId)
            .HasColumnName("purchase_order_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(20)
            .IsRequired();

        b.HasIndex(x => x.OrderNumber).IsUnique();

        b.Property(x => x.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        b.Property(x => x.StoreId)
            .HasColumnName("store_id")
            .IsRequired();

        b.Property(x => x.OrderDate)
            .HasColumnName("order_date")
            .IsRequired();

        b.Property(x => x.DesiredDeliveryDate)
            .HasColumnName("desired_delivery_date");

        b.Property(x => x.ExpectedDeliveryDate)
            .HasColumnName("expected_delivery_date");

        b.Property(x => x.ReceivedDate)
            .HasColumnName("received_date");

        b.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        b.Property(x => x.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(12, 2)
            .IsRequired();

        b.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        b.Property(x => x.ApprovedBy)
            .HasColumnName("approved_by");

        b.Property(x => x.ApprovedAt)
            .HasColumnName("approved_at");

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
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

        b.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Approver)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Details)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
