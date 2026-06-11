using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Registry.Application.Abstractions;
using PetHotel.Registry.Application.Pets.RegisterPet;
using PetHotel.Registry.Application.Pets.UpdatePet;
using PetHotel.Registry.Application.Tutors.RegisterTutor;
using PetHotel.Registry.Application.Tutors.UpdateTutor;
using PetHotel.Registry.Domain.Ports;
using PetHotel.Registry.Infrastructure.Persistence;
using PetHotel.Registry.Infrastructure.Persistence.Repositories;

namespace PetHotel.Registry.Infrastructure;

/// <summary>Ponto único de registro DI do módulo Registry (docs/02).</summary>
public static class RegistryModuleExtensions
{
    public static IServiceCollection AddRegistryModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<RegistryDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", RegistryDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, RegistryUnitOfWork>();

        services.AddScoped<ITutorRepository, TutorRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<ITutorQueries, TutorQueries>();
        services.AddScoped<IPetQueries, PetQueries>();

        services.AddScoped<IValidator<RegisterTutor>, RegisterTutorValidator>();
        services.AddScoped<IValidator<RegisterPet>, RegisterPetValidator>();
        services.AddScoped<IValidator<UpdateTutor>, UpdateTutorValidator>();
        services.AddScoped<IValidator<UpdatePet>, UpdatePetValidator>();

        return services;
    }
}
