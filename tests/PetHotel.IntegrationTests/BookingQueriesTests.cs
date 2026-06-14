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
/// Listas de leitura do Booking (acomodações e reservas) contra um PostgreSQL real
/// (docs/04/06): respeito ao query filter de tenant e filtro por status.
/// </summary>
public sealed class BookingQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private readonly TestTenantContext _tenant = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Acomodacoes_sao_isoladas_por_tenant_e_ordenadas_por_nome()
    {
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();

        _tenant.Current = tenantA;
        await SeedAccommodationAsync(tenantA, "Suíte A");
        await SeedAccommodationAsync(tenantA, "Box 1");

        _tenant.Current = tenantB;
        await SeedAccommodationAsync(tenantB, "Box do B");

        _tenant.Current = tenantA;
        await using var context = CreateContext();
        var result = await new AccommodationQueries(context).ListAsync();

        Assert.Equal(["Box 1", "Suíte A"], result.Select(a => a.Name));
    }

    [Fact]
    public async Task Reservas_filtram_por_status_e_respeitam_o_tenant()
    {
        var tenant = TenantId.New();
        _tenant.Current = tenant;

        var accommodationId = await SeedAccommodationAsync(tenant, "Box 1");
        await SeedReservationAsync(tenant, accommodationId, confirmed: false);
        await SeedReservationAsync(tenant, accommodationId, confirmed: true);

        // Outro tenant não deve aparecer.
        var otherTenant = TenantId.New();
        _tenant.Current = otherTenant;
        var otherAccommodation = await SeedAccommodationAsync(otherTenant, "Box do outro");
        await SeedReservationAsync(otherTenant, otherAccommodation, confirmed: false);

        _tenant.Current = tenant;
        await using var context = CreateContext();
        var queries = new ReservationQueries(context);

        var all = await queries.ListAsync(null, null, null, null, 1, 20);
        Assert.Equal(2, all.Total);
        Assert.Equal(2, all.Items.Count);
        Assert.Single((await queries.ListAsync(ReservationStatus.Requested, null, null, null, 1, 20)).Items);
        var confirmed = await queries.ListAsync(ReservationStatus.Confirmed, null, null, null, 1, 20);
        Assert.Single(confirmed.Items);
        Assert.Equal("Confirmed", confirmed.Items[0].Status);

        // Paginação por offset: página 1 com tamanho 1 traz 1 item, mas o total reflete os 2.
        var firstPage = await queries.ListAsync(null, null, null, null, 1, 1);
        Assert.Single(firstPage.Items);
        Assert.Equal(2, firstPage.Total);
        Assert.Equal(2, firstPage.TotalPages);
        var secondPage = await queries.ListAsync(null, null, null, null, 2, 1);
        Assert.Single(secondPage.Items);
        Assert.NotEqual(firstPage.Items[0].Id, secondPage.Items[0].Id);
    }

    private async Task<Guid> SeedAccommodationAsync(TenantId tenant, string name)
    {
        await using var context = CreateContext();
        var accommodation = Accommodation.Create(tenant, name, 100m, 1).Value;
        context.Accommodations.Add(accommodation);
        await context.SaveChangesAsync();
        return accommodation.Id.Value;
    }

    private async Task SeedReservationAsync(TenantId tenant, Guid accommodationId, bool confirmed)
    {
        await using var context = CreateContext();
        var period = DateRange.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)).Value;
        var reservation = Reservation
            .Request(tenant, new PetReference(Guid.NewGuid()), new AccommodationId(accommodationId), period, 100m)
            .Value;

        if (confirmed)
        {
            reservation.Confirm(isPetHealthCleared: true);
        }

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();
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
