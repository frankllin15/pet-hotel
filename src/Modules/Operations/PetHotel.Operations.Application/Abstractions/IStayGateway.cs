namespace PetHotel.Operations.Application.Abstractions;

/// <summary>
/// Porta de saída do Operations para consultar a estadia (reserva). O adaptador chama o
/// contrato público do Booking — o Operations NUNCA lê tabelas do Booking (docs/01).
/// </summary>
public interface IStayGateway
{
    Task<StaySnapshot?> FindStayAsync(Guid reservationId, CancellationToken cancellationToken = default);
}

/// <summary>Estadia vista pelo Operations. <see cref="Arrived"/> = pet presente (check-in feito).</summary>
public sealed record StaySnapshot(Guid PetId, bool Arrived);
