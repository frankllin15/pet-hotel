using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetHotel.Operations.Domain.Tasks;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence.Configurations;

/// <summary>Mapeamento do agregado <see cref="OperationalTask"/> (Fluent API, docs/04).</summary>
public sealed class OperationalTaskEntityConfiguration : IEntityTypeConfiguration<OperationalTask>
{
    public void Configure(EntityTypeBuilder<OperationalTask> builder)
    {
        builder.ToTable("operational_tasks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new OperationalTaskId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Date).IsRequired();
        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.AssignedTo).HasColumnName("assigned_to");
        builder.Property(t => t.Done).IsRequired();
        builder.Property(t => t.CompletedAt);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(t => new { t.TenantId, t.Date });

        builder.Ignore(t => t.DomainEvents);
    }
}
