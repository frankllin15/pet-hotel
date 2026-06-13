namespace PetHotel.Booking.Application.Accommodations.CreateAccommodation;

/// <summary>Cria uma unidade reservável (acomodação) no tenant corrente.</summary>
public sealed record CreateAccommodation(string Name, decimal DailyRate);
