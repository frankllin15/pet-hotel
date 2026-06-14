namespace PetHotel.Booking.Application.Reservations.ListReservations;

/// <summary>
/// Lista reservas do tenant corrente, paginadas por offset. Filtros opcionais: estado,
/// acomodação e janela de período (reservas que se sobrepõem a <paramref name="From"/>..<paramref name="To"/>).
/// </summary>
public sealed record ListReservations(
    string? Status = null,
    Guid? AccommodationId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 20);
