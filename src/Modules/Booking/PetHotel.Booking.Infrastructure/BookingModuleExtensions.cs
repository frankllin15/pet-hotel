using Microsoft.Extensions.DependencyInjection;

namespace PetHotel.Booking.Infrastructure;

/// <summary>
/// Ponto único de registro DI do módulo Booking (docs/02). Portas são registradas
/// pelo módulo que as implementa.
/// </summary>
public static class BookingModuleExtensions
{
    public static IServiceCollection AddBookingModule(this IServiceCollection services, string connectionString)
    {
        // TODO: registrar BookingDbContext (AddDbContextPool), repositórios e adaptadores.
        return services;
    }
}
