using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations.Events;

/// <summary>O pet deu saída da hospedagem (check-out). Encerramento da estadia.</summary>
public sealed record ReservationCheckedOut(
    ReservationId ReservationId,
    TenantId TenantId,
    PetReference Pet,
    DateTimeOffset OccurredAt) : IDomainEvent;