using PetHotel.Booking.Application.Abstractions;
using PetHotel.Registry.Application.Contracts;

namespace PetHotel.Booking.Infrastructure.Adapters;

/// <summary>
/// Adaptador da porta de compatibilidade do Booking sobre o contrato público do Registry.
/// O Booking não lê tabelas do Registry — só fala pelo contrato (docs/01).
/// </summary>
public sealed class BookingPetCompatibilityGateway(IPetCompatibilityContract compatibility) : IPetCompatibilityGateway
{
    public async Task<IReadOnlyList<PetCompatibilityInfo>> GetCompatibilityAsync(
        IReadOnlyCollection<Guid> petIds, CancellationToken cancellationToken = default)
    {
        var result = await compatibility.GetCompatibilityAsync(petIds, cancellationToken);
        return result.Select(p => new PetCompatibilityInfo(p.PetId, p.Name, p.Flags)).ToList();
    }
}
