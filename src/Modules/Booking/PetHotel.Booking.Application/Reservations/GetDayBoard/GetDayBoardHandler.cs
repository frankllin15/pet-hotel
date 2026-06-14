using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Reservations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Application.Reservations.GetDayBoard;

/// <summary>Projeta o painel do dia direto para DTO (docs/04).</summary>
public static class GetDayBoardHandler
{
    public static async Task<Result<DayBoardDto>> Handle(
        GetDayBoard query,
        IDayBoardQueries dayBoard,
        CancellationToken cancellationToken)
    {
        var board = await dayBoard.GetAsync(query.Date, cancellationToken);
        return Result.Success(board);
    }
}
