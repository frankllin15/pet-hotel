using PetHotel.Booking.Application.Contracts;
using PetHotel.Operations.Application.Abstractions;

namespace PetHotel.Operations.Infrastructure.Adapters;

/// <summary>
/// Adaptador da porta de estadia do Operations sobre o contrato público do Booking.
/// O Operations não lê tabelas do Booking — só fala pelo contrato (docs/01).
/// </summary>
public sealed class OperationsStayGateway(IStayContract stays) : IStayGateway
{
    public async Task<StaySnapshot?> FindStayAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var stay = await stays.FindStayAsync(reservationId, cancellationToken);
        return stay is null ? null : new StaySnapshot(stay.PetId, stay.Arrived);
    }
}
