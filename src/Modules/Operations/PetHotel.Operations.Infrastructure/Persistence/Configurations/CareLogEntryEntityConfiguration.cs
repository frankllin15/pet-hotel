using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="CareLogEntry"/> (Fluent API, docs/04).</summary>
public sealed class CareLogEntryEntityConfiguration : IEntityTypeConfiguration<CareLogEntry>
{
    public void Configure(EntityTypeBuilder<CareLogEntry> builder)
    {
        builder.ToTable("care_log_entries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new CareLogEntryId(value))
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(e => e.Pet)
            .HasConversion(pet => pet.Value, value => new PetReference(value))
            .HasColumnName("pet_id")
            .IsRequired();

        builder.Property(e => e.ContextType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.ContextId).HasColumnName("context_id").IsRequired();

        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.PrimitiveCollection(e => e.PhotoKeys);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(200);
        builder.Property(e => e.UpdatedBy).HasMaxLength(200);

        // Índice para a timeline por contexto (estadia) dentro do tenant (ordenada por ocorrência).
        builder.HasIndex(e => new { e.TenantId, e.ContextId, e.OccurredAt });

        builder.Ignore(e => e.DomainEvents);
    }
}
