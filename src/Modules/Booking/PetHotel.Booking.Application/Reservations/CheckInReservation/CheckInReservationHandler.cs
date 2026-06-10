using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.CheckInReservation;

/// <summary>Faz o check-in da reserva (Confirmed → CheckedIn), carimbando o horário real de entrada.</summary>
public static class CheckInReservationHandler
{
    public static async Task<Result> Handle(
        CheckInReservation command,
        IReservationRepository reservations,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.FindAsync(new ReservationId(command.ReservationId), cancellationToken);
        if (reservation is null)
        {
            return Error.NotFound("reservation.not_found", "Reserva não encontrada.");
        }

        var result = reservation.CheckIn(clock.GetUtcNow());
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}