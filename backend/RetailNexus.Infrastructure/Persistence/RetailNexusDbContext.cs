using Microsoft.EntityFrameworkCore;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence;

public class RetailNexusDbContext : DbContext
{
    public RetailNexusDbContext(DbContextOptions<RetailNexusDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailNexusDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
