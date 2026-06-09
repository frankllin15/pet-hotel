using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations.Events;

/// <summary>Uma reserva foi confirmada (clearance sanitário OK).</summary>
public sealed record ReservationConfirmed(ReservationId ReservationId, TenantId TenantId, PetReference Pet) : IDomainEvent;
