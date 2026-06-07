using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Pet"/> (Fluent API, docs/04).</summary>
public sealed class PetEntityConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("pets");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PetId(value))
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(p => p.TutorId)
            .HasConversion(id => id.Value, value => new TutorId(value))
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.Species)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Breed).HasMaxLength(120);
        builder.Property(p => p.BirthDate);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);

        // Índice para listar pets por tutor dentro do tenant.
        builder.HasIndex(p => new { p.TenantId, p.TutorId });

        builder.Ignore(p => p.DomainEvents);
    }
}
