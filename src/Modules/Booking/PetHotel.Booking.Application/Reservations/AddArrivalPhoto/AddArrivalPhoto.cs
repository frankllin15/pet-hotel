namespace PetHotel.Booking.Application.Reservations.AddArrivalPhoto;

/// <summary>Anexa uma foto de chegada (já gravada no storage) a uma reserva em estadia/encerrada.</summary>
public sealed record AddArrivalPhoto(Guid ReservationId, string Key);
