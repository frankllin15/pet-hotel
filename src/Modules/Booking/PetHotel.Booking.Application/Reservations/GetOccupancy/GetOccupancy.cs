namespace PetHotel.Booking.Application.Reservations.GetOccupancy;

/// <summary>Calendário de ocupação (reservas confirmadas) em um intervalo.</summary>
public sealed record GetOccupancy(DateOnly From, DateOnly To);
