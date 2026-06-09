using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations.Events;

/// <summary>Uma reserva foi cancelada.</summary>
public sealed record ReservationCancelled(ReservationId ReservationId, TenantId TenantId) : IDomainEvent;
