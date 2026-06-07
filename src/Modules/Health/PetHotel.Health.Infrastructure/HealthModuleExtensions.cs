using Microsoft.Extensions.DependencyInjection;

namespace PetHotel.Health.Infrastructure;

/// <summary>
/// Ponto único de registro DI do módulo Health (docs/02). Portas são registradas
/// pelo módulo que as implementa.
/// </summary>
public static class HealthModuleExtensions
{
    public static IServiceCollection AddHealthModule(this IServiceCollection services, string connectionString)
    {
        // TODO: registrar HealthDbContext (AddDbContextPool), repositórios e adaptadores.
        return services;
    }
}
