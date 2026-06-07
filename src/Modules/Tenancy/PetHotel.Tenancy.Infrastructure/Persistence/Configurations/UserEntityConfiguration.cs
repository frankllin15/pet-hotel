using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Users;

namespace PetHotel.Tenancy.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="User"/> (Fluent API, docs/04).</summary>
public sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(u => u.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(200);
        builder.Property(u => u.UpdatedBy).HasMaxLength(200);

        // E-mail único por tenant (índice combinado com o discriminador, docs/04).
        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();

        builder.Ignore(u => u.DomainEvents);
    }
}
