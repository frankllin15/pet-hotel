namespace PetHotel.Booking.Application.Reservations.RemoveArrivalPhoto;

/// <summary>Remove uma foto de chegada de uma reserva (pela chave do storage).</summary>
public sealed record RemoveArrivalPhoto(Guid ReservationId, string Key);
