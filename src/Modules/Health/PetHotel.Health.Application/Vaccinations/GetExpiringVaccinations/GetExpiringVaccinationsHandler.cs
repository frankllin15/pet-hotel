using PetHotel.Health.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Health.Application.Vaccinations.GetExpiringVaccinations;

/// <summary>Projeta os alertas de vacina direto para DTO (docs/04).</summary>
public static class GetExpiringVaccinationsHandler
{
    public static async Task<Result<IReadOnlyList<ExpiringVaccinationDto>>> Handle(
        GetExpiringVaccinations query,
        IVaccinationAlertQueries alerts,
        CancellationToken cancellationToken)
    {
        var rows = await alerts.GetExpiringAsync(query.AsOf, query.WithinDays, cancellationToken);
        return Result.Success(rows);
    }
}
