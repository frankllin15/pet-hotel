using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.IntegrationTests.Support;
using PetHotel.SharedKernel;
using Testcontainers.PostgreSql;

namespace PetHotel.IntegrationTests;

/// <summary>
/// Caso obrigatório de concorrência (docs/06): duas confirmações concorrentes que tocam
/// a mesma acomodação → uma falha por conflito de xmin (impede overbooking).
/// </summary>
public sealed class BookingConcurrencyTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private readonly TestTenantContext _tenant = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _tenant.Current = TenantId.New();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Confirmacoes_concorrentes_na_mesma_acomodacao_uma_falha_por_xmin()
    {
        var period1 = DateRange.Create(new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 14)).Value;
        var period2 = DateRange.Create(new DateOnly(2026, 7, 12), new DateOnly(2026, 7, 16)).Value;

        AccommodationId accommodationId;
        ReservationId firstId;
        ReservationId secondId;

        await using (var context = CreateContext())
        {
            var accommodation = Accommodation.Create(_tenant.Current, "Box concorrência", 100m).Value;
            var first = Reservation.Request(_tenant.Current, new PetReference(Guid.NewGuid()), accommodation.Id, period1, 100m).Value;
            var second = Reservation.Request(_tenant.Current, new PetReference(Guid.NewGuid()), accommodation.Id, period2, 100m).Value;

            context.Accommodations.Add(accommodation);
            context.Reservations.Add(first);
            context.Reservations.Add(second);
            await context.SaveChangesAsync();

            accommodationId = accommodation.Id;
            firstId = first.Id;
            secondId = second.Id;
        }

        // Dois contextos leem a MESMA acomodação (mesmo xmin) e confirmam em paralelo.
        await using var contextA = CreateContext();
        await using var contextB = CreateContext();

        var accA = await contextA.Accommodations.FirstAsync(a => a.Id == accommodationId);
        var accB = await contextB.Accommodations.FirstAsync(a => a.Id == accommodationId);
        var reservationA = await contextA.Reservations.FirstAsync(r => r.Id == firstId);
        var reservationB = await contextB.Reservations.FirstAsync(r => r.Id == secondId);

        var now = DateTimeOffset.UtcNow;
        reservationA.Confirm(true);
        accA.MarkBooked(now);
        reservationB.Confirm(true);
        accB.MarkBooked(now);

        await contextA.SaveChangesAsync(); // primeira confirmação vence

        // A segunda colide no xmin da acomodação.
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());
    }

    private BookingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .AddInterceptors(new TenantAuditingInterceptor(_tenant, new TestCurrentUser(), TimeProvider.System))
            .Options;

        return new BookingDbContext(options, _tenant);
    }
}
