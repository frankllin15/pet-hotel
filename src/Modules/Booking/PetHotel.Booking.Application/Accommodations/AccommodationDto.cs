namespace PetHotel.Booking.Application.Accommodations;

/// <summary>Projeção de leitura de uma acomodação.</summary>
public sealed record AccommodationDto(Guid Id, string Name, decimal DailyRate, int Capacity, string Status);
