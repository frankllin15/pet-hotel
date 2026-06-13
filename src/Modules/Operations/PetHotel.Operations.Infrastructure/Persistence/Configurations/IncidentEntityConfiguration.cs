using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Incidents;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="Incident"/> (Fluent API, docs/04).</summary>
public sealed class IncidentEntityConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new IncidentId(value))
            .ValueGeneratedNever();

        builder.Property(i => i.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(i => i.Pet)
            .HasConversion(pet => pet.Value, value => new PetReference(value))
            .HasColumnName("pet_id")
            .IsRequired();

        builder.Property(i => i.ContextType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(i => i.ContextId).HasColumnName("context_id").IsRequired();

        builder.Property(i => i.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(2000).IsRequired();
        builder.Property(i => i.OccurredAt).IsRequired();

        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(200);
        builder.Property(i => i.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(i => new { i.TenantId, i.ContextId, i.OccurredAt });

        builder.Ignore(i => i.DomainEvents);
    }
}
