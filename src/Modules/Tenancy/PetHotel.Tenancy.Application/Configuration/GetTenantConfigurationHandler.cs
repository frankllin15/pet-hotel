using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Ports;

namespace PetHotel.Tenancy.Application.Configuration;

/// <summary>Lê a configuração do tenant corrente e projeta para DTO (docs/04).</summary>
public static class GetTenantConfigurationHandler
{
    public static async Task<Result<TenantConfigurationDto>> Handle(
        GetTenantConfiguration query,
        ITenantConfigurationRepository configurations,
        CancellationToken cancellationToken)
    {
        var config = await configurations.GetForCurrentTenantAsync(cancellationToken);
        if (config is null)
        {
            return Error.NotFound("configuration.not_found", "Configuração não encontrada.");
        }

        return new TenantConfigurationDto(
            config.AccommodationTypes
                .Select(a => new AccommodationTypeDto(a.Name, a.Capacity, a.DailyPrice))
                .ToList(),
            config.RequiredVaccines.ToList(),
            config.CheckInTime,
            config.CheckOutTime,
            config.SetupCompleted);
    }
}
