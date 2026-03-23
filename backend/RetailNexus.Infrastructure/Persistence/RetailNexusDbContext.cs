using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence;

public class RetailNexusDbContext : DbContext
{
    public RetailNexusDbContext(DbContextOptions<RetailNexusDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreType> StoreTypes => Set<StoreType>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailNexusDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
