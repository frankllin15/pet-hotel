namespace PetHotel.Booking.Application.Reservations.CreateReservation;

/// <summary>Solicita uma reserva para um pet em uma acomodação, em um período.</summary>
public sealed record CreateReservation(Guid PetId, Guid AccommodationId, DateOnly CheckIn, DateOnly CheckOut);
