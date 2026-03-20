using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> b)
    {
        b.ToTable("product_categories");

        b.HasKey(x => x.ProductCategoryId);

        b.Property(x => x.ProductCategoryId)
            .HasColumnName("product_category_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.ProductCategoryCd)
            .HasColumnName("product_category_cd")
            .HasMaxLength(30)
            .IsRequired();
        b.HasIndex(x => x.ProductCategoryCd).IsUnique();

        b.Property(x => x.ProductCategoryName)
            .HasColumnName("product_category_name")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.DisplayOrder)
            .HasColumnName("display_order")
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
    }
}