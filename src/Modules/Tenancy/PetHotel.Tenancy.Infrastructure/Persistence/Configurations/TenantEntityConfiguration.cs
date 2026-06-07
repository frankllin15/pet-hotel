using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Tenants;

namespace PetHotel.Tenancy.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Tenant"/> (Fluent API, docs/04).</summary>
public sealed class TenantEntityConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasColumnName("slug")
            .HasMaxLength(63)
            .IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();

        builder.Ignore(t => t.DomainEvents);
    }
}
