using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;
using Wolverine.Attributes;

namespace PetHotel.Booking.Application.Reservations.ConfirmReservation;

/// <summary>
/// Confirma a reserva: consulta a aptidão sanitária do pet (gateway → contrato do Health)
/// na data de check-in, revalida sobreposição e confirma. A concorrência otimista (xmin)
/// em Accommodation serializa confirmações concorrentes e impede overbooking (docs/03, docs/04).
/// </summary>
/// <remarks>
/// Fora do Wolverine (<see cref="WolverineIgnoreAttribute"/>): toca dois DbContexts
/// (Booking na escrita + Health no gateway de leitura), então a auto-transação do Wolverine
/// não consegue decidir o contexto. Invocado direto via DI no endpoint.
/// </remarks>
[WolverineIgnore]
public static class ConfirmReservationHandler
{
    public static async Task<Result> Handle(
        ConfirmReservation command,
        IReservationRepository reservations,
        IAccommodationRepository accommodations,
        IHealthClearanceGateway healthClearance,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var reservationId = new ReservationId(command.ReservationId);

        var reservation = await reservations.FindAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return Error.NotFound("reservation.not_found", "Reserva não encontrada.");
        }

        if (await reservations.HasConfirmedOverlapAsync(
                reservation.AccommodationId, reservation.Period, reservation.Id, cancellationToken))
        {
            return Error.Conflict("accommodation.unavailable", "Já há reserva confirmada para o período nessa acomodação.");
        }

        var accommodation = await accommodations.FindAsync(reservation.AccommodationId, cancellationToken);
        if (accommodation is null)
        {
            return Error.NotFound("accommodation.not_found", "Acomodação não encontrada.");
        }

        var cleared = await healthClearance.IsPetClearedAsync(
            reservation.Pet.Value, reservation.Period.Start, cancellationToken);

        var result = reservation.Confirm(cleared);
        if (result.IsFailure)
        {
            return result.Error;
        }

        // Toca a acomodação para acionar o check de concorrência (xmin) entre confirmações.
        accommodation.MarkBooked(clock.GetUtcNow());

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict("reservation.concurrency", "Confirmação concorrente detectada. Tente novamente.");
        }

        return Result.Success();
    }
}
