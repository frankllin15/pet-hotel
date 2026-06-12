using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Health.Application.Abstractions;
using PetHotel.Health.Application.Contracts;
using PetHotel.Health.Application.ParasiteTreatments.RegisterParasiteTreatment;
using PetHotel.Health.Application.ParasiteTreatments.UpdateParasiteTreatment;
using PetHotel.Health.Application.Vaccinations.RegisterVaccination;
using PetHotel.Health.Application.Vaccinations.UpdateVaccination;
using PetHotel.Health.Application.VetContacts.SetVetContact;
using PetHotel.Health.Domain.Ports;
using PetHotel.Health.Infrastructure.Persistence;
using PetHotel.Health.Infrastructure.Persistence.Repositories;

namespace PetHotel.Health.Infrastructure;

/// <summary>Ponto único de registro DI do módulo Health (docs/02).</summary>
public static class HealthModuleExtensions
{
    public static IServiceCollection AddHealthModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<HealthDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", HealthDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, HealthUnitOfWork>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();

        // Contrato público consumido por outros módulos (ex.: Booking).
        services.AddScoped<IHealthClearanceContract, HealthClearanceContract>();

        services.AddScoped<IValidator<RegisterVaccination>, RegisterVaccinationValidator>();
        services.AddScoped<IValidator<UpdateVaccination>, UpdateVaccinationValidator>();
        services.AddScoped<IValidator<RegisterParasiteTreatment>, RegisterParasiteTreatmentValidator>();
        services.AddScoped<IValidator<UpdateParasiteTreatment>, UpdateParasiteTreatmentValidator>();
        services.AddScoped<IValidator<SetVetContact>, SetVetContactValidator>();

        return services;
    }
}
