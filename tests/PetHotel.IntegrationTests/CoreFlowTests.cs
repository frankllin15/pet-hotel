using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetHotel.Api.Http;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.Health.Infrastructure.Persistence;
using PetHotel.IntegrationTests.Support;
using PetHotel.Registry.Infrastructure.Persistence;
using PetHotel.Tenancy.Application.Provisioning;
using PetHotel.Tenancy.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PetHotel.IntegrationTests;

/// <summary>
/// Critério de "núcleo pronto" (docs/07): cadastrar tutor → pet → criar reserva →
/// confirmar BLOQUEADA por falta de vacina → vacinar → confirmar OK → ver no calendário.
/// Exercita Registry + Health + Booking e o gateway de clearance entre módulos.
/// </summary>
[Collection(WebApplicationCollection.Name)]
public sealed class CoreFlowTests : IAsyncLifetime
{
    private const string PlatformKey = "test-platform-key";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    private static readonly string[] EnvKeys =
        ["ConnectionStrings__Postgres", "PlatformKey", "Jwt__Issuer", "Jwt__Audience", "Jwt__SigningKey", "Jwt__LifetimeMinutes"];

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("PlatformKey", PlatformKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "PetHotel");
        Environment.SetEnvironmentVariable("Jwt__Audience", "PetHotel");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "core-flow-test-signing-key-long-enough-0123456789-abcdef");
        Environment.SetEnvironmentVariable("Jwt__LifetimeMinutes", "60");

        _factory = new WebApplicationFactory<Program>();

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<TenancyDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<RegistryDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<HealthDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<BookingDbContext>().Database.MigrateAsync();
        }

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();

        foreach (var key in EnvKeys)
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    [Fact]
    public async Task Fluxo_nucleo_reserva_bloqueada_por_vacina_e_confirmada_apos_vacinar()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkIn = today.AddDays(1);
        var checkOut = today.AddDays(3);

        // Provisionar + ativar + login (admin).
        var provision = new HttpRequestMessage(HttpMethod.Post, "/v1/provisioning/tenants")
        {
            Content = JsonContent.Create(new
            {
                name = "Hotel Núcleo",
                slug = "hotel-nucleo",
                adminEmail = "admin@nucleo.com",
                adminDisplayName = "Admin"
            })
        };
        provision.Headers.Add("X-Platform-Key", PlatformKey);
        var provisioned = await (await _client.SendAsync(provision)).Content.ReadFromJsonAsync<ProvisionedTenant>();

        await _client.PostAsJsonAsync("/v1/auth/activate", new
        {
            email = "admin@nucleo.com",
            token = provisioned!.ActivationToken,
            password = "S3nh@Forte!"
        });

        var login = await _client.PostAsJsonAsync("/v1/auth/login", new { email = "admin@nucleo.com", password = "S3nh@Forte!" });
        var token = (await login.Content.ReadFromJsonAsync<AccessTokenResponse>())!.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Cadastros: acomodação, tutor, pet.
        var accommodationId = await CreateAsync("/v1/accommodations", new { name = "Suíte 1" });
        var tutorId = await CreateAsync("/v1/tutors", new { fullName = "Maria", email = "maria@nucleo.com", phone = "11999990000" });
        var petId = await CreateAsync("/v1/pets", new { tutorId, name = "Rex", species = "Dog", breed = (string?)null, birthDate = (DateOnly?)null, notes = (string?)null });

        // Criar reserva.
        var reservationId = await CreateAsync("/v1/reservations", new
        {
            petId,
            accommodationId,
            checkIn,
            checkOut
        });

        // Confirmar SEM vacina → 409 (vaccine.expired).
        var blocked = await _client.PostAsync($"/v1/reservations/{reservationId}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        // Registrar antirrábica vigente cobrindo o check-in.
        var vaccinate = await _client.PostAsJsonAsync($"/v1/pets/{petId}/vaccinations", new
        {
            type = "Rabies",
            appliedOn = today,
            expiresOn = today.AddYears(1)
        });
        Assert.Equal(HttpStatusCode.Created, vaccinate.StatusCode);

        // Confirmar novamente → 204.
        var confirmed = await _client.PostAsync($"/v1/reservations/{reservationId}/confirm", null);
        Assert.Equal(HttpStatusCode.NoContent, confirmed.StatusCode);

        // Calendário de ocupação contém a reserva.
        var occupancy = await _client.GetFromJsonAsync<List<OccupancyEntryDto>>(
            $"/v1/occupancy?from={today:yyyy-MM-dd}&to={today.AddDays(30):yyyy-MM-dd}");
        Assert.NotNull(occupancy);
        Assert.Contains(occupancy!, e => e.ReservationId == reservationId && e.PetId == petId);
    }

    private async Task<Guid> CreateAsync(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    private sealed record AccessTokenResponse(string Token, DateTimeOffset ExpiresAt);
}
