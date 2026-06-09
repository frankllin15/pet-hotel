using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>
/// Concorrência otimista via a coluna de sistema <c>xmin</c> do PostgreSQL, sem coluna
/// extra (docs/04). Substitui o helper <c>UseXminAsConcurrencyToken</c> removido no Npgsql 10:
/// mapeia uma propriedade-sombra para a coluna de sistema (não criada na migration).
/// </summary>
public static class XminConcurrencyExtensions
{
    public static EntityTypeBuilder UseXminConcurrencyToken(this EntityTypeBuilder builder)
    {
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        return builder;
    }
}
