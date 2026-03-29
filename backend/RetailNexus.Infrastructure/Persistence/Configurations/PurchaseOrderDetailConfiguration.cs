using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> b)
    {
        b.ToTable("purchase_order_details");

        b.HasKey(x => x.PurchaseOrderDetailId);

        b.Property(x => x.PurchaseOrderDetailId)
            .HasColumnName("purchase_order_detail_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.PurchaseOrderId)
            .HasColumnName("purchase_order_id")
            .IsRequired();

        b.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        b.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        b.Property(x => x.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(12, 2)
            .IsRequired();

        b.Property(x => x.SubTotal)
            .HasColumnName("sub_total")
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

        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
