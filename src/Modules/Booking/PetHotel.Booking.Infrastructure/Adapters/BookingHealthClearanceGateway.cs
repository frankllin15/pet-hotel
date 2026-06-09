using PetHotel.Booking.Application.Abstractions;
using PetHotel.Health.Application.Contracts;

namespace PetHotel.Booking.Infrastructure.Adapters;

/// <summary>
/// Adaptador da porta de clearance do Booking sobre o contrato público do Health.
/// O Booking não lê tabelas do Health — só fala pelo contrato (docs/01).
/// </summary>
public sealed class BookingHealthClearanceGateway(IHealthClearanceContract clearance) : IHealthClearanceGateway
{
    public async Task<bool> IsPetClearedAsync(Guid petId, DateOnly asOf, CancellationToken cancellationToken = default)
    {
        var result = await clearance.GetClearanceAsync(petId, asOf, cancellationToken);
        return result.IsCleared;
    }
}
