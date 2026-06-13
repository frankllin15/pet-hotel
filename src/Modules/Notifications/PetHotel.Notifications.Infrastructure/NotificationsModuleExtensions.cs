using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Application.Reports.CreateReport;
using PetHotel.Notifications.Domain.Ports;
using PetHotel.Notifications.Infrastructure.Persistence;
using PetHotel.Notifications.Infrastructure.Persistence.Repositories;

namespace PetHotel.Notifications.Infrastructure;

/// <summary>Ponto único de registro DI do módulo Notifications (docs/02).</summary>
public static class NotificationsModuleExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, NpgsqlDataSource dataSource)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<NotificationsDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(dataSource, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", NotificationsDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, NotificationsUnitOfWork>();
        services.AddScoped<IOutboundMessageRepository, OutboundMessageRepository>();
        services.AddScoped<IOutboundMessageQueries, OutboundMessageQueries>();

        services.AddScoped<IValidator<CreateReport>, CreateReportValidator>();

        return services;
    }
}
