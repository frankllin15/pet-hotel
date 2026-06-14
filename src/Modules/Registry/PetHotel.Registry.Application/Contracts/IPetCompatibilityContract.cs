namespace PetHotel.Registry.Application.Contracts;

/// <summary>
/// Contrato PÚBLICO do módulo Registry. Outros módulos (ex.: Booking) consultam por aqui a
/// compatibilidade comportamental dos pets em vez de ler tabelas do Registry (docs/01).
/// Implementado pela Infrastructure do Registry.
/// </summary>
public interface IPetCompatibilityContract
{
    /// <summary>
    /// Sinais de compatibilidade comportamental dos pets informados, no tenant corrente.
    /// Pets sem ficha/avaliação voltam com lista de flags vazia (ou ausentes, se removidos).
    /// </summary>
    Task<IReadOnlyList<PetCompatibility>> GetCompatibilityAsync(
        IReadOnlyCollection<Guid> petIds, CancellationToken cancellationToken = default);
}
