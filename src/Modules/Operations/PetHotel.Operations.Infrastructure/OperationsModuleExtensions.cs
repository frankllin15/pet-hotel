using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.CareLog.LogCareEntry;
using PetHotel.Operations.Domain.Ports;
using PetHotel.Operations.Infrastructure.Adapters;
using PetHotel.Operations.Infrastructure.Persistence;
using PetHotel.Operations.Infrastructure.Persistence.Repositories;

namespace PetHotel.Operations.Infrastructure;

/// <summary>Ponto único de registro DI do módulo Operations (docs/02).</summary>
public static class OperationsModuleExtensions
{
    public static IServiceCollection AddOperationsModule(this IServiceCollection services, NpgsqlDataSource dataSource)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<OperationsDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(dataSource, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", OperationsDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, OperationsUnitOfWork>();
        services.AddScoped<ICareLogRepository, CareLogRepository>();
        services.AddScoped<ICareLogQueries, CareLogQueries>();
        services.AddScoped<IStayGateway, OperationsStayGateway>();

        services.AddScoped<IValidator<LogCareEntry>, LogCareEntryValidator>();

        return services;
    }
}
