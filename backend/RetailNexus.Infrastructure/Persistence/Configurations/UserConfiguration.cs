using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.UserId);

        b.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.LoginId)
            .HasColumnName("login_id")
            .HasMaxLength(50)
            .IsRequired();

        b.HasIndex(x => x.LoginId)
            .IsUnique();

        b.Property(x => x.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        b.HasIndex(x => x.Email)
            .IsUnique();

        b.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        b.HasIndex(x => x.IsActive);

        b.Property(x => x.LastLoginAt)
            .HasColumnName("last_login_at");

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by");
    }
}