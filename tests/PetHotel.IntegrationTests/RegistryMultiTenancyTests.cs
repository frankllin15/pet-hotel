using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.IntegrationTests.Support;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.Registry.Infrastructure.Persistence;
using PetHotel.SharedKernel;
using Testcontainers.PostgreSql;

namespace PetHotel.IntegrationTests;

/// <summary>
/// Casos obrigatórios de multi-tenancy contra um PostgreSQL real (docs/06):
/// isolamento por query filter e carimbo de auditoria pelo interceptor.
/// </summary>
public sealed class RegistryMultiTenancyTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
        .Build();

    private readonly TestTenantContext _tenant = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Leitura_de_um_tenant_nunca_retorna_dado_de_outro()
    {
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();

        // Escreve um tutor no tenant A.
        _tenant.Current = tenantA;
        await using (var context = CreateContext())
        {
            context.Tutors.Add(Tutor.Register(tenantA, "Tenant A", "a@hotel.com", "11999990000").Value);
            await context.SaveChangesAsync();
        }

        // Tenant B não enxerga nada.
        _tenant.Current = tenantB;
        await using (var context = CreateContext())
        {
            Assert.Empty(await context.Tutors.ToListAsync());
        }

        // Tenant A enxerga o seu.
        _tenant.Current = tenantA;
        await using (var context = CreateContext())
        {
            var tutors = await context.Tutors.ToListAsync();
            Assert.Single(tutors);
            Assert.Equal(tenantA, tutors[0].TenantId);
        }
    }

    [Fact]
    public async Task Interceptor_preenche_auditoria_na_insercao()
    {
        var tenant = TenantId.New();
        _tenant.Current = tenant;

        Guid id;
        await using (var context = CreateContext())
        {
            var tutor = Tutor.Register(tenant, "Auditado", "audit@hotel.com", "11999991111").Value;
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();
            id = tutor.Id.Value;
        }

        await using (var context = CreateContext())
        {
            var tutor = await context.Tutors.SingleAsync(t => t.Id == new TutorId(id));
            // CreatedAt é preenchido pelo interceptor, não pelo agregado.
            Assert.NotEqual(default, tutor.CreatedAt);
        }
    }

    private RegistryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RegistryDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .AddInterceptors(new TenantAuditingInterceptor(_tenant, new TestCurrentUser(), TimeProvider.System))
            .Options;

        return new RegistryDbContext(options, _tenant);
    }
}
