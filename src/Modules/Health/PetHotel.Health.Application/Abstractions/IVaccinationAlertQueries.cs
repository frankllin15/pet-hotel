using PetHotel.Health.Application.Vaccinations;

namespace PetHotel.Health.Application.Abstractions;

/// <summary>Porta de leitura de alertas de vacina (vencida/a vencer) (docs/04).</summary>
public interface IVaccinationAlertQueries
{
    /// <summary>
    /// Vacinas vencidas ou que vencem em até <paramref name="withinDays"/> dias a partir de
    /// <paramref name="asOf"/>, no tenant corrente, ordenadas por validade.
    /// </summary>
    Task<IReadOnlyList<ExpiringVaccinationDto>> GetExpiringAsync(
        DateOnly asOf, int withinDays, CancellationToken cancellationToken = default);
}
