namespace PetHotel.Booking.Application.Accommodations.UpdateAccommodation;

/// <summary>Edita uma acomodação (nome, diária e disponibilidade) no tenant corrente.</summary>
public sealed record UpdateAccommodation(Guid Id, string Name, decimal DailyRate, bool Active);
