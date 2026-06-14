namespace PetHotel.Booking.Application.Abstractions;

/// <summary>
/// Porta de saída do Booking para consultar a compatibilidade comportamental de pets. O
/// adaptador (Infrastructure) chama o contrato público do Registry — o Booking NUNCA lê
/// tabelas do Registry (docs/01).
/// </summary>
public interface IPetCompatibilityGateway
{
    Task<IReadOnlyList<PetCompatibilityInfo>> GetCompatibilityAsync(
        IReadOnlyCollection<Guid> petIds, CancellationToken cancellationToken = default);
}

/// <summary>Compatibilidade de um pet vista pelo Booking (nome + flags como texto).</summary>
public sealed record PetCompatibilityInfo(Guid PetId, string Name, IReadOnlyList<string> Flags);
