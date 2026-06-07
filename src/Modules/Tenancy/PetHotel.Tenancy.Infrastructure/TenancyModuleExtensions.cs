using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Tenants.RegisterTenant;
using PetHotel.Tenancy.Application.Users.RegisterUser;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Infrastructure.Persistence;
using PetHotel.Tenancy.Infrastructure.Persistence.Repositories;

namespace PetHotel.Tenancy.Infrastructure;

/// <summary>
/// Ponto único de registro DI do módulo Tenancy (docs/02). Portas registradas
/// pelo módulo que as implementa.
/// </summary>
public static class TenancyModuleExtensions
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<TenancyDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", TenancyDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        // O DbContext é a Unit of Work do módulo (docs/04), exposto por adaptador concreto.
        services.AddScoped<IUnitOfWork, TenancyUnitOfWork>();

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantQueries, TenantQueries>();

        services.AddScoped<IValidator<RegisterTenant>, RegisterTenantValidator>();
        services.AddScoped<IValidator<RegisterUser>, RegisterUserValidator>();

        return services;
    }
}
