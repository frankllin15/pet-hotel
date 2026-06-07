using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento da entidade interna <see cref="Vaccination"/> (docs/04).</summary>
public sealed class VaccinationEntityConfiguration : IEntityTypeConfiguration<Vaccination>
{
    public void Configure(EntityTypeBuilder<Vaccination> builder)
    {
        builder.ToTable("vaccinations");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasConversion(id => id.Value, value => new VaccinationId(value))
            .ValueGeneratedNever();

        builder.Property(v => v.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(v => v.AppliedOn).IsRequired();
        builder.Property(v => v.ExpiresOn).IsRequired();

        builder.HasIndex("HealthRecordId");
    }
}
