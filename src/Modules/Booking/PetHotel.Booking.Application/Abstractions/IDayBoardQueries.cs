using PetHotel.Booking.Application.Reservations;

namespace PetHotel.Booking.Application.Abstractions;

/// <summary>Porta de leitura do painel do dia (chegadas/saídas/estadia + ocupação) (docs/04).</summary>
public interface IDayBoardQueries
{
    /// <summary>Consolida o panorama do dia informado para o tenant corrente.</summary>
    Task<DayBoardDto> GetAsync(DateOnly date, CancellationToken cancellationToken = default);
}
