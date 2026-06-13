using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetHotel.Api.Http;
using PetHotel.Booking.Application.Accommodations;
using PetHotel.Booking.Application.Reservations;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.Health.Infrastructure.Persistence;
using PetHotel.IntegrationTests.Support;
using PetHotel.Notifications.Infrastructure.Persistence;
using PetHotel.Operations.Infrastructure.Persistence;
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
            await scope.ServiceProvider.GetRequiredService<OperationsDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();
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
        var accommodationId = await CreateAsync("/v1/accommodations", new { name = "Suíte 1", dailyRate = 150m });
        var tutorId = await CreateAsync("/v1/tutors", new { fullName = "Maria", email = "maria@nucleo.com", phone = "11999990000" });

        // LGPD: registra consentimentos e relê na ficha (valida wiring/validator + jsonb + projeção).
        var consents = await _client.PutAsJsonAsync($"/v1/tutors/{tutorId}/consents", new
        {
            consents = new[]
            {
                new { type = "ImageUse", granted = true },
                new { type = "Marketing", granted = false },
            }
        });
        Assert.Equal(HttpStatusCode.NoContent, consents.StatusCode);

        var tutorJson = await _client.GetFromJsonAsync<JsonElement>($"/v1/tutors/{tutorId}");
        var consentArray = tutorJson.GetProperty("consents");
        Assert.Equal(2, consentArray.GetArrayLength());
        var imageUse = consentArray.EnumerateArray().Single(c => c.GetProperty("type").GetString() == "ImageUse");
        Assert.True(imageUse.GetProperty("granted").GetBoolean());
        Assert.Equal("1.0", imageUse.GetProperty("termsVersion").GetString());
        var petId = await CreateAsync("/v1/pets", new { tutorId, name = "Rex", species = "Dog", breed = (string?)null, birthDate = (DateOnly?)null, notes = (string?)null });

        // Criar reserva.
        var reservationId = await CreateAsync("/v1/reservations", new
        {
            petId,
            accommodationId,
            checkIn,
            checkOut
        });

        // Precificação: total = diária (150) × noites (2 = check-in+1 a check-in+3) = 300.
        var created = await _client.GetFromJsonAsync<ReservationDto>($"/v1/reservations/{reservationId}");
        Assert.Equal(2, created!.Nights);
        Assert.Equal(150m, created.DailyRate);
        Assert.Equal(300m, created.TotalAmount);

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

        // Check-in com estado de chegada (corpo opcional) → 204; estado persiste e volta na leitura.
        var checkInResponse = await _client.PostAsJsonAsync($"/v1/reservations/{reservationId}/check-in", new
        {
            weightKg = 9.2m,
            condition = "MinorIssues",
            observations = "Chegou agitado"
        });
        Assert.Equal(HttpStatusCode.NoContent, checkInResponse.StatusCode);

        var reservations = await _client.GetFromJsonAsync<List<ReservationDto>>("/v1/reservations");
        var checkedIn = reservations!.Single(r => r.Id == reservationId);
        Assert.Equal("CheckedIn", checkedIn.Status);
        Assert.NotNull(checkedIn.ArrivalState);
        Assert.Equal(9.2m, checkedIn.ArrivalState!.WeightKg);
        Assert.Equal("MinorIssues", checkedIn.ArrivalState.Condition);
        Assert.Equal("Chegou agitado", checkedIn.ArrivalState.Observations);

        // Detalhe da reserva por Id reflete o mesmo estado.
        var detail = await _client.GetFromJsonAsync<ReservationDto>($"/v1/reservations/{reservationId}");
        Assert.Equal("CheckedIn", detail!.Status);
        Assert.Equal("Chegou agitado", detail.ArrivalState!.Observations);

        // Id inexistente → 404.
        var missing = await _client.GetAsync($"/v1/reservations/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        // Foto de chegada (multipart, reserva em estadia) → 200; aparece na ficha.
        using (var form = new MultipartFormDataContent())
        {
            var img = new ByteArrayContent([1, 2, 3, 4]);
            img.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(img, "file", "chegada.png");
            var upload = await _client.PostAsync($"/v1/reservations/{reservationId}/arrival-photos", form);
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        }

        var withPhoto = await _client.GetFromJsonAsync<ReservationDto>($"/v1/reservations/{reservationId}");
        Assert.Single(withPhoto!.ArrivalPhotoUrls);

        // Regressão (overbooking): uma reserva ENCERRADA libera a vaga — novo período
        // sobreposto na mesma acomodação NÃO é mais bloqueado.
        var checkOutResponse = await _client.PostAsync($"/v1/reservations/{reservationId}/check-out", null);
        Assert.Equal(HttpStatusCode.NoContent, checkOutResponse.StatusCode);

        var rebook = await _client.PostAsJsonAsync("/v1/reservations", new { petId, accommodationId, checkIn, checkOut });
        Assert.Equal(HttpStatusCode.Created, rebook.StatusCode);
        var rebookId = (await rebook.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;

        // Edição da acomodação (renomear + nova diária) → 204; reflete na listagem.
        var editAcc = await _client.PutAsJsonAsync($"/v1/accommodations/{accommodationId}", new
        {
            name = "Suíte Master",
            dailyRate = 200m,
            active = true
        });
        Assert.Equal(HttpStatusCode.NoContent, editAcc.StatusCode);

        var accommodations = await _client.GetFromJsonAsync<List<AccommodationDto>>("/v1/accommodations");
        var edited = accommodations!.Single(a => a.Id == accommodationId);
        Assert.Equal("Suíte Master", edited.Name);
        Assert.Equal(200m, edited.DailyRate);

        // Matilhas: cria com o pet (sem alerta), torna o pet reativo e relê (alerta de compatibilidade).
        var packId = await CreateAsync("/v1/packs", new { name = "Matilha A", notes = (string?)null, memberPetIds = new[] { petId } });

        var pack1 = await _client.GetFromJsonAsync<JsonElement>($"/v1/packs/{packId}");
        Assert.False(pack1.GetProperty("needsAttention").GetBoolean());
        Assert.Equal(1, pack1.GetProperty("members").GetArrayLength());

        var setBehavior = await _client.PutAsJsonAsync($"/v1/pets/{petId}", new { name = "Rex", species = "Dog", reactivity = "High" });
        Assert.Equal(HttpStatusCode.NoContent, setBehavior.StatusCode);

        var pack2 = await _client.GetFromJsonAsync<JsonElement>($"/v1/packs/{packId}");
        Assert.True(pack2.GetProperty("needsAttention").GetBoolean());
        var flags = pack2.GetProperty("members")[0].GetProperty("flags").EnumerateArray().Select(f => f.GetString());
        Assert.Contains("Reactive", flags);

        // Operations: diário vinculado à estadia. A reserva original está encerrada (pet chegou) →
        // registra e relê a timeline; a reserva nova ainda é Solicitada (pet não chegou) → 409.
        var log = await _client.PostAsJsonAsync($"/v1/reservations/{reservationId}/care-log", new { type = "Meal", note = "comeu tudo" });
        Assert.Equal(HttpStatusCode.Created, log.StatusCode);
        var entryId = (await log.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;

        var careLog = await _client.GetFromJsonAsync<JsonElement>($"/v1/reservations/{reservationId}/care-log");
        Assert.Equal(1, careLog.GetProperty("items").GetArrayLength());
        var entry = careLog.GetProperty("items")[0];
        Assert.Equal("Meal", entry.GetProperty("type").GetString());
        Assert.Equal("comeu tudo", entry.GetProperty("note").GetString());

        var blockedLog = await _client.PostAsJsonAsync($"/v1/reservations/{rebookId}/care-log", new { type = "Meal", note = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, blockedLog.StatusCode);

        // Foto na ocorrência (multipart) → 200; aparece na timeline.
        using (var form = new MultipartFormDataContent())
        {
            var img = new ByteArrayContent([5, 6, 7, 8]);
            img.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(img, "file", "diario.png");
            var photo = await _client.PostAsync($"/v1/care-log/{entryId}/photos", form);
            Assert.Equal(HttpStatusCode.OK, photo.StatusCode);
        }
        var careLog2 = await _client.GetFromJsonAsync<JsonElement>($"/v1/reservations/{reservationId}/care-log");
        Assert.Equal(1, careLog2.GetProperty("items")[0].GetProperty("photoUrls").GetArrayLength());

        // Medicação e incidente na estadia → 201; aparecem nas listas.
        var med = await _client.PostAsJsonAsync($"/v1/reservations/{reservationId}/medications", new { drug = "Dipirona", dose = "1 comprimido" });
        Assert.Equal(HttpStatusCode.Created, med.StatusCode);
        var meds = await _client.GetFromJsonAsync<JsonElement>($"/v1/reservations/{reservationId}/medications");
        Assert.Equal(1, meds.GetArrayLength());
        Assert.Equal("Dipirona", meds[0].GetProperty("drug").GetString());

        // Diretório de usuários resolve a autoria (givenBy → nome de exibição).
        var givenBy = meds[0].GetProperty("givenBy").GetString();
        var users = await _client.GetFromJsonAsync<JsonElement>("/v1/users");
        var actor = users.EnumerateArray().Single(u => u.GetProperty("id").GetString() == givenBy);
        Assert.Equal("Admin", actor.GetProperty("displayName").GetString());

        var incident = await _client.PostAsJsonAsync($"/v1/reservations/{reservationId}/incidents", new { severity = "High", description = "Brigou com outro pet" });
        Assert.Equal(HttpStatusCode.Created, incident.StatusCode);
        var incidents = await _client.GetFromJsonAsync<JsonElement>($"/v1/reservations/{reservationId}/incidents");
        Assert.Equal(1, incidents.GetArrayLength());
        Assert.Equal("High", incidents[0].GetProperty("severity").GetString());

        // Regra de estadia vale também p/ medicação/incidente: reserva Solicitada → 409.
        var blockedMed = await _client.PostAsJsonAsync($"/v1/reservations/{rebookId}/medications", new { drug = "X", dose = "1" });
        Assert.Equal(HttpStatusCode.Conflict, blockedMed.StatusCode);

        // Notifications: cria relatório do dia (rascunho), marca enviado, relê histórico por estadia e por tutor.
        var reportId = await CreateAsync("/v1/reports", new
        {
            tutorId,
            petId,
            reservationId,
            reportDate = today,
            title = "Relatório do Rex",
            content = "Comeu tudo; tomou Dipirona; brigou com outro pet."
        });

        var send = await _client.PostAsync($"/v1/reports/{reportId}/send", null);
        Assert.Equal(HttpStatusCode.NoContent, send.StatusCode);

        var stayReports = await _client.GetFromJsonAsync<JsonElement>($"/v1/reservations/{reservationId}/reports");
        Assert.Equal(1, stayReports.GetArrayLength());
        Assert.Equal("Sent", stayReports[0].GetProperty("status").GetString());

        var tutorReports = await _client.GetFromJsonAsync<JsonElement>($"/v1/tutors/{tutorId}/reports");
        Assert.Equal(1, tutorReports.GetArrayLength());
        Assert.Equal("Relatório do Rex", tutorReports[0].GetProperty("title").GetString());
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
