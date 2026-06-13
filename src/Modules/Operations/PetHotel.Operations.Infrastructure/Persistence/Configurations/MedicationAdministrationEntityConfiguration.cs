using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Medications;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="MedicationAdministration"/> (Fluent API, docs/04).</summary>
public sealed class MedicationAdministrationEntityConfiguration : IEntityTypeConfiguration<MedicationAdministration>
{
    public void Configure(EntityTypeBuilder<MedicationAdministration> builder)
    {
        builder.ToTable("medications");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => new MedicationAdministrationId(value))
            .ValueGeneratedNever();

        builder.Property(m => m.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(m => m.Pet)
            .HasConversion(pet => pet.Value, value => new PetReference(value))
            .HasColumnName("pet_id")
            .IsRequired();

        builder.Property(m => m.ContextType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(m => m.ContextId).HasColumnName("context_id").IsRequired();

        builder.Property(m => m.Drug).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Dose).HasMaxLength(100).IsRequired();
        builder.Property(m => m.AdministeredAt).IsRequired();

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(m => new { m.TenantId, m.ContextId, m.AdministeredAt });

        builder.Ignore(m => m.DomainEvents);
    }
}
