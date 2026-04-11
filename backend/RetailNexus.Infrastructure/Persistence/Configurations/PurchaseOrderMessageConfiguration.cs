using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderMessageConfiguration : IEntityTypeConfiguration<PurchaseOrderMessage>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderMessage> b)
    {
        b.ToTable("purchase_order_messages");

        b.HasKey(x => x.PurchaseOrderMessageId);

        b.Property(x => x.PurchaseOrderMessageId)
            .HasColumnName("purchase_order_message_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.PurchaseOrderId)
            .HasColumnName("purchase_order_id")
            .IsRequired();

        b.Property(x => x.SentBy)
            .HasColumnName("sent_by")
            .IsRequired();

        b.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(500)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SentBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
