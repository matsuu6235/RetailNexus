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

        b.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.ProductCode).IsUnique();
        b.Property(x => x.JanCode).HasMaxLength(32);
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ProductCategoryCode).HasMaxLength(50);

        b.Property(x => x.Price).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Cost).HasPrecision(12, 2).IsRequired();

        b.Property(x => x.IsActive).IsRequired();

        b.Property(x => x.UpdatedAt).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
    }
}