using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class StoreRequestConfiguration : IEntityTypeConfiguration<StoreRequest>
{
    public void Configure(EntityTypeBuilder<StoreRequest> b)
    {
        b.ToTable("store_requests");

        b.HasKey(x => x.StoreRequestId);

        b.Property(x => x.StoreRequestId)
            .HasColumnName("store_request_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.RequestNumber)
            .HasColumnName("request_number")
            .HasMaxLength(20)
            .IsRequired();

        b.HasIndex(x => x.RequestNumber).IsUnique();

        b.Property(x => x.FromStoreId)
            .HasColumnName("from_store_id")
            .IsRequired();

        b.Property(x => x.ToStoreId)
            .HasColumnName("to_store_id")
            .IsRequired();

        b.Property(x => x.RequestDate)
            .HasColumnName("request_date")
            .IsRequired();

        b.Property(x => x.DesiredDeliveryDate)
            .HasColumnName("desired_delivery_date");

        b.Property(x => x.ExpectedDeliveryDate)
            .HasColumnName("expected_delivery_date");

        b.Property(x => x.ShippedDate)
            .HasColumnName("shipped_date");

        b.Property(x => x.ReceivedDate)
            .HasColumnName("received_date");

        b.Property(x => x.Status)
            .HasColumnName("status")
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

        b.HasOne(x => x.FromStore)
            .WithMany()
            .HasForeignKey(x => x.FromStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ToStore)
            .WithMany()
            .HasForeignKey(x => x.ToStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Approver)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Details)
            .WithOne(x => x.StoreRequest)
            .HasForeignKey(x => x.StoreRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
