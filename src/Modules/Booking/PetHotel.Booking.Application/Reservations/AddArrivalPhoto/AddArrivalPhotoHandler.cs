using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.AddArrivalPhoto;

/// <summary>Registra a chave da foto de chegada na reserva (o arquivo já está no storage).</summary>
public static class AddArrivalPhotoHandler
{
    public static async Task<Result> Handle(
        AddArrivalPhoto command,
        IReservationRepository reservations,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.FindAsync(new ReservationId(command.ReservationId), cancellationToken);
        if (reservation is null)
        {
            return Error.NotFound("reservation.not_found", "Reserva não encontrada.");
        }

        var result = reservation.AddArrivalPhoto(command.Key);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
