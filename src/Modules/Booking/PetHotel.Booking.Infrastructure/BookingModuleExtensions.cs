using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Application.Accommodations.CreateAccommodation;
using PetHotel.Booking.Application.Accommodations.UpdateAccommodation;
using PetHotel.Booking.Application.Reservations.CreateReservation;
using PetHotel.Booking.Domain.Ports;
using PetHotel.Booking.Infrastructure.Adapters;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.Booking.Infrastructure.Persistence.Repositories;
using PetHotel.BuildingBlocks.Persistence;

namespace PetHotel.Booking.Infrastructure;

/// <summary>Ponto único de registro DI do módulo Booking (docs/02).</summary>
public static class BookingModuleExtensions
{
    public static IServiceCollection AddBookingModule(this IServiceCollection services, NpgsqlDataSource dataSource)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<BookingDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(dataSource, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BookingDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        services.AddScoped<IUnitOfWork, BookingUnitOfWork>();
        services.AddScoped<IAccommodationRepository, AccommodationRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IOccupancyQueries, OccupancyQueries>();
        services.AddScoped<IAccommodationQueries, AccommodationQueries>();
        services.AddScoped<IReservationQueries, ReservationQueries>();

        // Adaptador do gateway sobre o contrato público do Health.
        services.AddScoped<IHealthClearanceGateway, BookingHealthClearanceGateway>();

        services.AddScoped<IValidator<CreateAccommodation>, CreateAccommodationValidator>();
        services.AddScoped<IValidator<UpdateAccommodation>, UpdateAccommodationValidator>();
        services.AddScoped<IValidator<CreateReservation>, CreateReservationValidator>();

        return services;
    }
}
