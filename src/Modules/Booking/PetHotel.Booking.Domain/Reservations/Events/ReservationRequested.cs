using PetHotel.Booking.Domain.Accommodations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations.Events;

/// <summary>Uma reserva foi solicitada (ainda não confirmada).</summary>
public sealed record ReservationRequested(
    ReservationId ReservationId,
    TenantId TenantId,
    PetReference Pet,
    AccommodationId AccommodationId) : IDomainEvent;
