namespace PetHotel.Booking.Application.Abstractions;

/// <summary>
/// Porta de saída do Booking para consultar a aptidão sanitária de um pet. O adaptador
/// (Infrastructure) chama o contrato público do módulo Health — o Booking NUNCA lê
/// tabelas do Health (docs/01).
/// </summary>
public interface IHealthClearanceGateway
{
    Task<bool> IsPetClearedAsync(Guid petId, DateOnly asOf, CancellationToken cancellationToken = default);
}
