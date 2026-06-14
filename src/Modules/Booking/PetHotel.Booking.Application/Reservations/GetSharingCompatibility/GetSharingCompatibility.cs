namespace PetHotel.Booking.Application.Reservations.GetSharingCompatibility;

/// <summary>
/// Consulta o alerta de compatibilidade ao colocar <see cref="PetId"/> na acomodação no período
/// (pré-visualização, não-bloqueante).
/// </summary>
public sealed record GetSharingCompatibility(Guid AccommodationId, DateOnly CheckIn, DateOnly CheckOut, Guid PetId);
