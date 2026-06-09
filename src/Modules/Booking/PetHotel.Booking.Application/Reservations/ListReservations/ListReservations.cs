namespace PetHotel.Booking.Application.Reservations.ListReservations;

/// <summary>
/// Lista reservas do tenant corrente. <paramref name="Status"/> opcional filtra por
/// estado (Requested / Confirmed / Cancelled).
/// </summary>
public sealed record ListReservations(string? Status = null);
