using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("product_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.ProductCode)
            .HasColumnName("product_code")
            .HasMaxLength(20)
            .IsRequired();

        b.HasIndex(x => x.ProductCode).IsUnique();

        b.Property(x => x.JanCode)
            .HasColumnName("jan_code")
            .HasMaxLength(13);

        b.Property(x => x.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.ProductCategoryCode)
            .HasColumnName("product_category_code")
            .HasMaxLength(50);

        b.Property(x => x.Price)
            .HasColumnName("price")
            .HasPrecision(12, 2)
            .IsRequired();

        b.Property(x => x.Cost)
            .HasColumnName("cost")
            .HasPrecision(12, 2)
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
