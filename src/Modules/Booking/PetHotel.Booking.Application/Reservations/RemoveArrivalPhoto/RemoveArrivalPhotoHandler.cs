using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.RemoveArrivalPhoto;

/// <summary>Remove a chave da foto de chegada da reserva (o arquivo é apagado pelo endpoint).</summary>
public static class RemoveArrivalPhotoHandler
{
    public static async Task<Result> Handle(
        RemoveArrivalPhoto command,
        IReservationRepository reservations,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.FindAsync(new ReservationId(command.ReservationId), cancellationToken);
        if (reservation is null)
        {
            return Error.NotFound("reservation.not_found", "Reserva não encontrada.");
        }

        var result = reservation.RemoveArrivalPhoto(command.Key);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
