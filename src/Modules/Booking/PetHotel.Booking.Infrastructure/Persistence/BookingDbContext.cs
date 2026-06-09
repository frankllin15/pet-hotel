using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>DbContext do módulo Booking (schema "booking", docs/04). É a Unit of Work.</summary>
public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IUnitOfWork
{
    public const string Schema = "booking";

    public DbSet<Accommodation> Accommodations => Set<Accommodation>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);

        // Aplica o tenant query filter depois das entidades mapeadas.
        base.OnModelCreating(modelBuilder);
    }
}
