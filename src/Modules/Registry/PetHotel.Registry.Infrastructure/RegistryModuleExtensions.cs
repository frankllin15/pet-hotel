using Microsoft.Extensions.DependencyInjection;

namespace PetHotel.Registry.Infrastructure;

/// <summary>
/// Ponto único de registro DI do módulo Registry (docs/02). Portas são registradas
/// pelo módulo que as implementa.
/// </summary>
public static class RegistryModuleExtensions
{
    public static IServiceCollection AddRegistryModule(this IServiceCollection services, string connectionString)
    {
        // TODO: registrar RegistryDbContext (AddDbContextPool), repositórios e adaptadores.
        return services;
    }
}
