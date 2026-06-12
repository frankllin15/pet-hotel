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

    /// <summary>Momento real do check-in (entrada do pet). Nulo enquanto não houve check-in.</summary>
    public DateTimeOffset? CheckedInAt { get; private set; }

    /// <summary>Momento real do check-out (saída do pet). Nulo enquanto não houve check-out.</summary>
    public DateTimeOffset? CheckedOutAt { get; private set; }

    /// <summary>Estado do pet registrado na chegada. Nulo se não foi informado no check-in.</summary>
    public ArrivalState? ArrivalState { get; private set; }

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

    /// <summary>
    /// Registra a entrada do pet (check-in). Só a partir de <see cref="ReservationStatus.Confirmed"/>:
    /// não se faz check-in de reserva apenas solicitada, já em estadia ou encerrada/cancelada.
    /// </summary>
    public Result CheckIn(DateTimeOffset now, ArrivalState? arrivalState = null)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            return Error.Conflict("reservation.invalid_state", "Só é possível fazer check-in de uma reserva confirmada.");
        }

        Status = ReservationStatus.CheckedIn;
        CheckedInAt = now;
        ArrivalState = arrivalState;
        Raise(new ReservationCheckedIn(Id, TenantId, Pet, now));
        return Result.Success();
    }

    /// <summary>Registra a saída do pet (check-out). Só a partir de <see cref="ReservationStatus.CheckedIn"/>.</summary>
    public Result CheckOut(DateTimeOffset now)
    {
        if (Status != ReservationStatus.CheckedIn)
        {
            return Error.Conflict("reservation.invalid_state", "Só é possível fazer check-out de uma reserva em estadia (com check-in).");
        }

        Status = ReservationStatus.CheckedOut;
        CheckedOutAt = now;
        Raise(new ReservationCheckedOut(Id, TenantId, Pet, now));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            return Error.Conflict("reservation.already_cancelled", "A reserva já está cancelada.");
        }

        // Uma vez iniciada a estadia (check-in), a reserva não é mais cancelável — segue para check-out.
        if (Status is ReservationStatus.CheckedIn or ReservationStatus.CheckedOut)
        {
            return Error.Conflict("reservation.invalid_state", "Não é possível cancelar uma reserva já em estadia ou encerrada.");
        }

        Status = ReservationStatus.Cancelled;
        Raise(new ReservationCancelled(Id, TenantId));
        return Result.Success();
    }
}
