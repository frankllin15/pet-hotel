using Microsoft.Extensions.DependencyInjection;

namespace PetHotel.Tenancy.Infrastructure;

/// <summary>
/// Ponto único de registro DI do módulo Tenancy (docs/02). Portas são registradas
/// pelo módulo que as implementa.
/// </summary>
public static class TenancyModuleExtensions
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services, string connectionString)
    {
        // TODO: registrar TenancyDbContext (AddDbContextPool), repositórios e adaptadores.
        return services;
    }
}
