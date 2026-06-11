using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento da entidade interna <see cref="ParasiteTreatment"/> (docs/04).</summary>
public sealed class ParasiteTreatmentEntityConfiguration : IEntityTypeConfiguration<ParasiteTreatment>
{
    public void Configure(EntityTypeBuilder<ParasiteTreatment> builder)
    {
        builder.ToTable("parasite_treatments");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new ParasiteTreatmentId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.ProductName).HasMaxLength(200);
        builder.Property(t => t.AppliedOn).IsRequired();
        builder.Property(t => t.NextDueOn);

        builder.HasIndex("HealthRecordId");
    }
}
