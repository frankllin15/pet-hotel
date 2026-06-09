using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.IntegrationTests.Support;
using PetHotel.Registry.Application.Tutors;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.Registry.Infrastructure.Persistence;
using PetHotel.SharedKernel;
using Testcontainers.PostgreSql;

namespace PetHotel.IntegrationTests;

/// <summary>
/// Paginação por cursor (keyset) do Registry contra um PostgreSQL real (docs/04/06):
/// valida a tradução EF do keyset, a ordem estável e o respeito ao query filter de tenant.
/// </summary>
public sealed class RegistryPaginationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private readonly TestTenantContext _tenant = new();

    // Avança o relógio a cada SaveChanges -> CreatedAt estritamente crescente (ordem determinística).
    private readonly IncrementingTimeProvider _clock = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Paginacao_percorre_todos_sem_duplicar_nem_pular_em_ordem_recente_primeiro()
    {
        var tenant = TenantId.New();
        _tenant.Current = tenant;

        // Inseridos do mais antigo para o mais novo.
        var insertedOrder = new[] { "Ana", "Bruno", "Carla", "Diego", "Eduardo" };
        foreach (var name in insertedOrder)
        {
            await SeedTutorAsync(tenant, name, $"{name.ToLowerInvariant()}@hotel.com");
        }

        // Pagina de 2 em 2 até esgotar.
        var collected = new List<string>();
        string? cursor = null;
        var pageCount = 0;
        do
        {
            var page = await ListAsync(search: null, cursor: cursor, limit: 2);
            collected.AddRange(page.Items.Select(t => t.FullName));
            cursor = page.NextCursor;
            pageCount++;
            Assert.True(pageCount <= 10, "Paginação não convergiu (possível loop).");
        }
        while (cursor is not null);

        // 3 páginas (2 + 2 + 1), sem duplicados, do mais novo para o mais antigo.
        Assert.Equal(3, pageCount);
        Assert.Equal(insertedOrder.Reverse(), collected);
        Assert.Equal(collected.Count, collected.Distinct().Count());
    }

    [Fact]
    public async Task Lista_respeita_o_query_filter_de_tenant()
    {
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();

        _tenant.Current = tenantA;
        await SeedTutorAsync(tenantA, "Do Tenant A", "a@hotel.com");

        _tenant.Current = tenantB;
        await SeedTutorAsync(tenantB, "Do Tenant B", "b@hotel.com");

        _tenant.Current = tenantA;
        var page = await ListAsync(search: null, cursor: null, limit: 20);

        Assert.Single(page.Items);
        Assert.Equal("Do Tenant A", page.Items[0].FullName);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task Filtro_de_busca_por_nome_e_case_insensitive()
    {
        var tenant = TenantId.New();
        _tenant.Current = tenant;

        await SeedTutorAsync(tenant, "Carla Mendes", "carla@hotel.com");
        await SeedTutorAsync(tenant, "Bruno Souza", "bruno@hotel.com");

        var page = await ListAsync(search: "carl", cursor: null, limit: 20);

        Assert.Single(page.Items);
        Assert.Equal("Carla Mendes", page.Items[0].FullName);
    }

    [Fact]
    public async Task Cursor_invalido_resulta_em_pagina_vazia_no_decode()
    {
        Assert.False(Cursor.TryDecode("not-a-cursor", out _));
    }

    private async Task SeedTutorAsync(TenantId tenant, string name, string email)
    {
        await using var context = CreateContext();
        context.Tutors.Add(Tutor.Register(tenant, name, email, "11999990000").Value);
        await context.SaveChangesAsync();
    }

    private async Task<CursorPage<TutorDto>> ListAsync(string? search, string? cursor, int limit)
    {
        await using var context = CreateContext();
        var queries = new TutorQueries(context);

        Cursor? after = null;
        if (cursor is not null && Cursor.TryDecode(cursor, out var decoded))
        {
            after = decoded;
        }

        return await queries.ListAsync(search, after, limit);
    }

    private RegistryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RegistryDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .AddInterceptors(new TenantAuditingInterceptor(_tenant, new TestCurrentUser(), _clock))
            .Options;

        return new RegistryDbContext(options, _tenant);
    }

    /// <summary>TimeProvider que avança 1s a cada leitura — garante CreatedAt único e crescente.</summary>
    private sealed class IncrementingTimeProvider : TimeProvider
    {
        private long _ticks = DateTimeOffset.UtcNow.UtcTicks;

        public override DateTimeOffset GetUtcNow()
        {
            _ticks += TimeSpan.TicksPerSecond;
            return new DateTimeOffset(_ticks, TimeSpan.Zero);
        }
    }
}
