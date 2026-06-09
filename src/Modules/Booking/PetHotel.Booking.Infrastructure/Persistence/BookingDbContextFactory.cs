using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>Factory para o EF Tools criarem migrations sem subir o host (docs/04, docs/07).</summary>
public sealed class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PETHOTEL_DB")
            ?? "Host=localhost;Port=5432;Database=pethotel;Username=postgres;Password=dev";

        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BookingDbContext.Schema))
            .Options;

        return new BookingDbContext(options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public TenantId Current => throw new InvalidOperationException("Sem tenant em design-time.");
        public bool HasTenant => false;
    }
}
