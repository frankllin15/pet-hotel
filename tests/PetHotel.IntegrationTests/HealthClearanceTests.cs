using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Infrastructure.Persistence;
using PetHotel.IntegrationTests.Support;
using PetHotel.SharedKernel;
using Testcontainers.PostgreSql;

namespace PetHotel.IntegrationTests;

/// <summary>
/// Verifica o contrato público de clearance contra um PostgreSQL real — é a peça
/// que o Booking vai consumir para bloquear reservas com vacina vencida (docs/01, docs/03).
/// </summary>
public sealed class HealthClearanceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private readonly TestTenantContext _tenant = new();
    private static readonly DateOnly Today = new(2026, 6, 7);

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Pet_sem_ficha_nao_esta_apto()
    {
        _tenant.Current = TenantId.New();
        var petId = Guid.NewGuid();

        var clearance = await new HealthClearanceContract(CreateContext()).GetClearanceAsync(petId, Today);

        Assert.False(clearance.IsCleared);
        Assert.Contains(nameof(VaccineType.Rabies), clearance.Pendencies);
    }

    [Fact]
    public async Task Antirrabica_vigente_deixa_pet_apto()
    {
        _tenant.Current = TenantId.New();
        var petId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var record = HealthRecord.Open(_tenant.Current, new PetReference(petId)).Value;
            record.AddVaccination(VaccineType.Rabies, Today.AddMonths(-1), Today.AddYears(1), Today);
            context.HealthRecords.Add(record);
            await context.SaveChangesAsync();
        }

        var clearance = await new HealthClearanceContract(CreateContext()).GetClearanceAsync(petId, Today);

        Assert.True(clearance.IsCleared);
        Assert.Empty(clearance.Pendencies);
    }

    [Fact]
    public async Task Antirrabica_vencida_bloqueia_aptidao()
    {
        _tenant.Current = TenantId.New();
        var petId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var record = HealthRecord.Open(_tenant.Current, new PetReference(petId)).Value;
            record.AddVaccination(VaccineType.Rabies, Today.AddYears(-2), Today.AddYears(-1), Today);
            context.HealthRecords.Add(record);
            await context.SaveChangesAsync();
        }

        var clearance = await new HealthClearanceContract(CreateContext()).GetClearanceAsync(petId, Today);

        Assert.False(clearance.IsCleared);
        Assert.Contains(nameof(VaccineType.Rabies), clearance.Pendencies);
    }

    private HealthDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HealthDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .AddInterceptors(new TenantAuditingInterceptor(_tenant, new TestCurrentUser(), TimeProvider.System))
            .Options;

        return new HealthDbContext(options, _tenant);
    }
}
