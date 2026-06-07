namespace PetHotel.Health.Application.Contracts;

/// <summary>
/// Contrato PÚBLICO do módulo Health. Outros módulos chamam por aqui em vez de ler
/// tabelas do Health (docs/01). Implementado pela Infrastructure do Health.
/// </summary>
public interface IHealthClearanceContract
{
    /// <summary>Aptidão sanitária do pet na data informada, no tenant corrente.</summary>
    Task<PetHealthClearance> GetClearanceAsync(Guid petId, DateOnly asOf, CancellationToken cancellationToken = default);
}
