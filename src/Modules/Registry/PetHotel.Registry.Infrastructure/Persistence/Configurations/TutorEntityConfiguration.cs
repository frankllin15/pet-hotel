using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Tutor"/> (Fluent API, docs/04).</summary>
public sealed class TutorEntityConfiguration : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("tutors");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TutorId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(t => t.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(t => t.Phone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value).Value)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(t => new { t.TenantId, t.Email }).IsUnique();

        builder.Ignore(t => t.DomainEvents);
    }
}
