using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetHotel.Registry.Infrastructure.Persistence;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Provisioning;
using PetHotel.Tenancy.Infrastructure.Persistence;
using PetHotel.IntegrationTests.Support;
using Testcontainers.PostgreSql;

namespace PetHotel.IntegrationTests;

/// <summary>
/// Fluxo de onboarding ponta a ponta via host real (WebApplicationFactory) contra um
/// PostgreSQL efêmero: provisionar → ativar → login → usar o JWT em endpoint protegido.
/// </summary>
[Collection(WebApplicationCollection.Name)]
public sealed class OnboardingFlowTests : IAsyncLifetime
{
    private const string PlatformKey = "test-platform-key";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    // Injetado por variável de ambiente porque o Program lê a connection string e o Jwt
    // no startup (antes do Build), fora do alcance do ConfigureAppConfiguration do WAF.
    private static readonly string[] EnvKeys =
        ["ConnectionStrings__Postgres", "PlatformKey", "Jwt__Issuer", "Jwt__Audience", "Jwt__SigningKey", "Jwt__LifetimeMinutes"];

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("PlatformKey", PlatformKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "PetHotel");
        Environment.SetEnvironmentVariable("Jwt__Audience", "PetHotel");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "integration-test-signing-key-long-enough-0123456789-abcdef");
        Environment.SetEnvironmentVariable("Jwt__LifetimeMinutes", "60");

        _factory = new WebApplicationFactory<Program>();

        // Aplica as migrations dos módulos exercitados (não rodamos migrations no startup).
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TenancyDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<RegistryDbContext>().Database.MigrateAsync();

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
    public async Task Provisionar_ativar_logar_e_acessar_endpoint_protegido()
    {
        // 1) Provisionamento (chave de plataforma).
        var provision = new HttpRequestMessage(HttpMethod.Post, "/v1/provisioning/tenants")
        {
            Content = JsonContent.Create(new
            {
                name = "Hotel Integração",
                slug = "hotel-integracao",
                adminEmail = "admin@integra.com",
                adminDisplayName = "Admin"
            })
        };
        provision.Headers.Add("X-Platform-Key", PlatformKey);

        var provisionResponse = await _client.SendAsync(provision);
        Assert.Equal(HttpStatusCode.Created, provisionResponse.StatusCode);
        var provisioned = await provisionResponse.Content.ReadFromJsonAsync<ProvisionedTenant>();
        Assert.NotNull(provisioned);

        // 2) Ativação (define a senha).
        var activate = await _client.PostAsJsonAsync("/v1/auth/activate", new
        {
            email = "admin@integra.com",
            token = provisioned!.ActivationToken,
            password = "S3nh@Forte!"
        });
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);

        // 3) Login → JWT.
        var login = await _client.PostAsJsonAsync("/v1/auth/login", new
        {
            email = "admin@integra.com",
            password = "S3nh@Forte!"
        });
        login.EnsureSuccessStatusCode();
        var accessToken = await login.Content.ReadFromJsonAsync<AccessToken>();
        Assert.NotNull(accessToken);

        // 4a) Endpoint protegido SEM token → 401.
        var anonymous = await _client.PostAsJsonAsync("/v1/tutors", new
        {
            fullName = "Tutor",
            email = "tutor@integra.com",
            phone = "11999990000"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        // 4b) Com o JWT (tenant vem do claim) → 201.
        var createTutor = new HttpRequestMessage(HttpMethod.Post, "/v1/tutors")
        {
            Content = JsonContent.Create(new
            {
                fullName = "Tutor",
                email = "tutor@integra.com",
                phone = "11999990000"
            })
        };
        createTutor.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken!.Token);

        var tutorResponse = await _client.SendAsync(createTutor);
        Assert.Equal(HttpStatusCode.Created, tutorResponse.StatusCode);
    }
}
