namespace PetHotel.Booking.Application.Reservations.GetSharingCompatibility;

/// <summary>
/// Resultado do alerta de compartilhamento de acomodação. <see cref="Shared"/> indica que há
/// outros pets na vaga no período; <see cref="Conflicts"/> traz os pets do grupo (incluindo o
/// candidato) que têm sinais comportamentais de atenção. É ALERTA, não bloqueio.
/// </summary>
public sealed record SharingCompatibilityDto(
    bool Shared,
    IReadOnlyList<PetCompatibilityDto> Conflicts);

/// <summary>Pet do grupo compartilhado com seus sinais de atenção (flags como texto).</summary>
public sealed record PetCompatibilityDto(Guid PetId, string Name, IReadOnlyList<string> Flags);
