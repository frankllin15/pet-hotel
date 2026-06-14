using FluentValidation;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Validation;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.CreateReservation;

/// <summary>
/// Solicita uma reserva (estado Requested). Valida acomodação disponível e ausência de
/// sobreposição com reservas confirmadas. A aptidão sanitária é checada na confirmação.
/// </summary>
public static class CreateReservationHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateReservation command,
        IValidator<CreateReservation> validator,
        ITenantContext tenantContext,
        IAccommodationRepository accommodations,
        IReservationRepository reservations,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var periodResult = DateRange.Create(command.CheckIn, command.CheckOut);
        if (periodResult.IsFailure)
        {
            return periodResult.Error;
        }

        var period = periodResult.Value;
        var accommodationId = new AccommodationId(command.AccommodationId);

        var accommodation = await accommodations.FindAsync(accommodationId, cancellationToken);
        if (accommodation is null)
        {
            return Error.NotFound("accommodation.not_found", "Acomodação não encontrada.");
        }

        if (!accommodation.IsAvailable)
        {
            return Error.Conflict("accommodation.inactive", "Acomodação indisponível.");
        }

        if (await reservations.CountActiveOverlapsAsync(accommodationId, period, null, cancellationToken)
            >= accommodation.Capacity)
        {
            return Error.Conflict("accommodation.unavailable", "Acomodação sem vaga para o período.");
        }

        var result = Reservation.Request(
            tenantContext.Current, new PetReference(command.PetId), accommodationId, period, accommodation.DailyRate);
        if (result.IsFailure)
        {
            return result.Error;
        }

        reservations.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
