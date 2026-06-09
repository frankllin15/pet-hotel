using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.CancelReservation;

/// <summary>Cancela uma reserva, liberando o período da acomodação.</summary>
public static class CancelReservationHandler
{
    public static async Task<Result> Handle(
        CancelReservation command,
        IReservationRepository reservations,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.FindAsync(new ReservationId(command.ReservationId), cancellationToken);
        if (reservation is null)
        {
            return Error.NotFound("reservation.not_found", "Reserva não encontrada.");
        }

        var result = reservation.Cancel();
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
