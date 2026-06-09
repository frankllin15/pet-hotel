using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations;

/// <summary>
/// Reserva de hospedagem. Agregado tenant-scoped; referencia pet e acomodação por Id.
/// A regra "vacina vencida bloqueia a reserva" é invariante de <see cref="Confirm"/> (docs/03).
/// </summary>
public sealed class Reservation : AggregateRoot<ReservationId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public PetReference Pet { get; private set; }
    public AccommodationId AccommodationId { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Reservation() { } // EF

    private Reservation(ReservationId id, TenantId tenantId, PetReference pet, AccommodationId accommodationId, DateRange period)
        : base(id)
    {
        TenantId = tenantId;
        Pet = pet;
        AccommodationId = accommodationId;
        Period = period;
        Status = ReservationStatus.Requested;
    }

    public static Result<Reservation> Request(
        TenantId tenantId, PetReference pet, AccommodationId accommodationId, DateRange period)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("reservation.tenant_required", "Tenant é obrigatório.");
        }

        if (pet.Value == Guid.Empty)
        {
            return Error.Validation("reservation.pet_required", "Pet é obrigatório.");
        }

        if (accommodationId.Value == Guid.Empty)
        {
            return Error.Validation("reservation.accommodation_required", "Acomodação é obrigatória.");
        }

        var reservation = new Reservation(ReservationId.New(), tenantId, pet, accommodationId, period);
        reservation.Raise(new ReservationRequested(reservation.Id, tenantId, pet, accommodationId));
        return reservation;
    }

    /// <summary>
    /// Confirma a reserva. Bloqueia se o pet não estiver apto (vacina obrigatória ausente
    /// ou vencida) ou se o estado não permitir.
    /// </summary>
    public Result Confirm(bool isPetHealthCleared)
    {
        if (Status != ReservationStatus.Requested)
        {
            return Error.Conflict("reservation.invalid_state", "A reserva não está em estado solicitável de confirmação.");
        }

        if (!isPetHealthCleared)
        {
            return Error.Conflict("vaccine.expired", "Pet sem aptidão sanitária (vacina obrigatória ausente ou vencida).");
        }

        Status = ReservationStatus.Confirmed;
        Raise(new ReservationConfirmed(Id, TenantId, Pet));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            return Error.Conflict("reservation.already_cancelled", "A reserva já está cancelada.");
        }

        Status = ReservationStatus.Cancelled;
        Raise(new ReservationCancelled(Id, TenantId));
        return Result.Success();
    }
}
