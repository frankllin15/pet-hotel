using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations.Events;

/// <summary>O pet deu entrada na hospedagem (check-in). Início efetivo da estadia.</summary>
public sealed record ReservationCheckedIn(
    ReservationId ReservationId,
    TenantId TenantId,
    PetReference Pet,
    DateTimeOffset OccurredAt) : IDomainEvent;