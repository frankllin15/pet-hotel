namespace PetHotel.Booking.Application.Contracts;

/// <summary>
/// Contrato PÚBLICO do módulo Booking: consulta de estadia (reserva) por outros módulos
/// (ex.: Operations, para vincular o diário). Quem consome NUNCA lê tabelas do Booking (docs/01).
/// </summary>
public interface IStayContract
{
    /// <summary>Estadia por Id da reserva no tenant corrente; null se não existir.</summary>
    Task<StayInfo?> FindStayAsync(Guid reservationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Estadia exposta a outros módulos. <see cref="Arrived"/> = o pet está/esteve presente
/// (check-in feito: em estadia ou já encerrada). Livre de tipos internos do Booking.
/// </summary>
public sealed record StayInfo(Guid ReservationId, Guid PetId, bool Arrived);
