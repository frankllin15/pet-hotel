using System.Diagnostics;
using System.Text.Json;
using Bogus;
using Microsoft.EntityFrameworkCore;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Reservations;
using PetHotel.Booking.Infrastructure.Persistence;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Infrastructure.Persistence;
using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.Registry.Infrastructure.Persistence;
using PetHotel.SharedKernel;
using PetHotel.LoadTest.Seeder;
using BookingPetRef = PetHotel.Booking.Domain.Reservations.PetReference;
using HealthPetRef = PetHotel.Health.Domain.HealthRecords.PetReference;

// ---------------------------------------------------------------------------
// Seeder multi-tenant para testes de carga. Insere dados realistas direto pelos
// DbContexts reais (mesmas conversões/jsonb/índices) e emite um JWT por tenant.
// Saída: um manifesto JSON consumido pelo k6 (loadtest/manifest.json).
//
// Config por variável de ambiente (defaults = dev local / appsettings.Development):
//   LOADTEST_CONNECTION  string de conexão Postgres
//   JWT_SIGNING_KEY / JWT_ISSUER / JWT_AUDIENCE
//   SEED_SCALE           multiplicador de volume (default 1.0)
//   SEED_OUTPUT          caminho do manifesto (default ../../loadtest/manifest.json)
// ---------------------------------------------------------------------------

var conn = Environment.GetEnvironmentVariable("LOADTEST_CONNECTION")
    ?? "Host=localhost;Port=5432;Database=pethotel;Username=postgres;Password=dev";
var signingKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
    ?? "dev-signing-key-please-change-this-to-a-long-random-secret-0123456789";
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PetHotel";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PetHotel";
var scale = double.TryParse(
    Environment.GetEnvironmentVariable("SEED_SCALE"),
    System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture,
    out var s) ? s : 1.0;
var outputPath = Environment.GetEnvironmentVariable("SEED_OUTPUT")
    ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "loadtest", "manifest.json");

// Distribuição assimétrica: poucos tenants grandes, vários médios, muitos pequenos —
// é o que estressa o planner do Postgres (estatísticas/índices por tenant).
const int BatchSize = 2000;

var tiers = new[]
{
    new Tier("large", Count: 2, Tutors: 1500, Accommodations: 100, Reservations: 6000, HealthFraction: 0.5),
    new Tier("medium", Count: 6, Tutors: 300, Accommodations: 30, Reservations: 1200, HealthFraction: 0.4),
    new Tier("small", Count: 12, Tutors: 40, Accommodations: 8, Reservations: 100, HealthFraction: 0.3),
};

var today = DateOnly.FromDateTime(DateTime.UtcNow);
var tenantCtx = new SeedTenantContext();
var interceptor = new TenantAuditingInterceptor(tenantCtx, new SeedCurrentUser(), TimeProvider.System);
var minter = new TokenMinter(issuer, audience, signingKey);
var tokenLifetime = TimeSpan.FromHours(12);

Randomizer.Seed = new Random(20260612); // reprodutível
var faker = new Faker("pt_BR");
var species = Enum.GetValues<Species>();
var sizes = Enum.GetValues<PetSize>();
var sexes = Enum.GetValues<Sex>();

var manifestTenants = new List<object>();
var totalSw = Stopwatch.StartNew();
var grand = (tenants: 0, tutors: 0, pets: 0, accommodations: 0, reservations: 0, health: 0);

RegistryDbContext NewRegistry() => new(
    new DbContextOptionsBuilder<RegistryDbContext>().UseNpgsql(conn).AddInterceptors(interceptor).Options, tenantCtx);
BookingDbContext NewBooking() => new(
    new DbContextOptionsBuilder<BookingDbContext>().UseNpgsql(conn).AddInterceptors(interceptor).Options, tenantCtx);
HealthDbContext NewHealth() => new(
    new DbContextOptionsBuilder<HealthDbContext>().UseNpgsql(conn).AddInterceptors(interceptor).Options, tenantCtx);

Console.WriteLine($"Seeder | scale={scale} | db={Mask(conn)}");

foreach (var tier in tiers)
{
    var tutorsTarget = Scaled(tier.Tutors);
    var accommodationsTarget = Scaled(tier.Accommodations);
    var reservationsTarget = Scaled(tier.Reservations);

    for (var t = 0; t < tier.Count; t++)
    {
        var tenantId = Guid.NewGuid();
        tenantCtx.Use(new TenantId(tenantId));
        var sw = Stopwatch.StartNew();

        // --- Registry: tutores + pets ---
        var petIds = new List<Guid>(tutorsTarget * 2);
        using (var reg = NewRegistry())
        {
            var tutorIds = new List<TutorId>(tutorsTarget);
            var tutorBatch = new List<Tutor>(BatchSize);
            for (var i = 0; i < tutorsTarget; i++)
            {
                var tutor = Tutor.Register(
                    new TenantId(tenantId),
                    faker.Name.FullName(),
                    $"tutor{i}@t{tenantId:N}.example.com", // único por tenant (índice TenantId,Email)
                    faker.Phone.PhoneNumber("###########")).Value;
                tutorIds.Add(tutor.Id);
                tutorBatch.Add(tutor);
                if (tutorBatch.Count >= BatchSize) await FlushAsync(reg, tutorBatch);
            }
            await FlushAsync(reg, tutorBatch);

            var petBatch = new List<Pet>(BatchSize);
            foreach (var tutorId in tutorIds)
            {
                var petsForTutor = faker.Random.Int(1, 3);
                for (var p = 0; p < petsForTutor; p++)
                {
                    var pet = Pet.Register(
                        new TenantId(tenantId), tutorId,
                        faker.Name.FirstName(), faker.PickRandom(species), faker.Commerce.ProductMaterial(),
                        DateOnly.FromDateTime(faker.Date.Past(12)), faker.PickRandom(sizes), faker.PickRandom(sexes),
                        faker.Random.Bool(), null, null, today).Value;
                    petIds.Add(pet.Id.Value);
                    petBatch.Add(pet);
                    if (petBatch.Count >= BatchSize) await FlushAsync(reg, petBatch);
                }
            }
            await FlushAsync(reg, petBatch);
            grand.tutors += tutorIds.Count;
            grand.pets += petIds.Count;
        }

        // --- Booking: acomodações + reservas (confirmadas/em estadia) ---
        using (var bk = NewBooking())
        {
            var accIds = new List<AccommodationId>(accommodationsTarget);
            var accBatch = new List<Accommodation>(BatchSize);
            for (var a = 0; a < accommodationsTarget; a++)
            {
                var acc = Accommodation.Create(new TenantId(tenantId), $"Box {a + 1}", faker.Random.Int(80, 300), faker.Random.Int(1, 4)).Value;
                accIds.Add(acc.Id);
                accBatch.Add(acc);
                if (accBatch.Count >= BatchSize) await FlushAsync(bk, accBatch);
            }
            await FlushAsync(bk, accBatch);

            var resBatch = new List<Reservation>(BatchSize);
            for (var r = 0; r < reservationsTarget && petIds.Count > 0; r++)
            {
                var start = today.AddDays(faker.Random.Int(-60, 60));
                var period = DateRange.Create(start, start.AddDays(faker.Random.Int(1, 14))).Value;
                var reservation = Reservation.Request(
                    new TenantId(tenantId),
                    new BookingPetRef(faker.PickRandom(petIds)),
                    faker.PickRandom(accIds),
                    period,
                    faker.Random.Int(80, 300)).Value;
                reservation.Confirm(true); // ocupa o calendário (status != Requested/Cancelled)
                if (faker.Random.Bool(0.3f))
                {
                    reservation.CheckIn(DateTime.UtcNow.AddDays(-faker.Random.Int(0, 5)));
                }
                resBatch.Add(reservation);
                if (resBatch.Count >= BatchSize) await FlushAsync(bk, resBatch);
            }
            await FlushAsync(bk, resBatch);
            grand.accommodations += accIds.Count;
            grand.reservations += reservationsTarget;
        }

        // --- Health: fichas + vacinas para um subconjunto dos pets ---
        var healthCount = (int)(petIds.Count * tier.HealthFraction);
        using (var he = NewHealth())
        {
            var recBatch = new List<HealthRecord>(BatchSize);
            foreach (var petId in petIds.Take(healthCount))
            {
                var record = HealthRecord.Open(new TenantId(tenantId), new HealthPetRef(petId)).Value;
                var applied = today.AddDays(-faker.Random.Int(0, 300));
                record.AddVaccination(VaccineType.Rabies, applied, applied.AddYears(1), today);
                if (faker.Random.Bool(0.5f))
                {
                    record.AddParasiteTreatment(
                        ParasiteTreatmentType.FleaTick, "Bravecto", applied, applied.AddMonths(3), today);
                }
                recBatch.Add(record);
                if (recBatch.Count >= BatchSize) await FlushAsync(he, recBatch);
            }
            await FlushAsync(he, recBatch);
            grand.health += healthCount;
        }

        manifestTenants.Add(new
        {
            tenantId,
            tier = tier.Name,
            token = minter.Issue(tenantId, tokenLifetime),
            petIds = petIds.Take(50).ToArray(), // amostra para reads de detalhe
            healthPetIds = petIds.Take(Math.Min(50, healthCount)).ToArray(), // pets que têm ficha (sem 404)
            occupancyFrom = today.AddDays(-30).ToString("yyyy-MM-dd"),
            occupancyTo = today.AddDays(30).ToString("yyyy-MM-dd"),
        });
        grand.tenants++;
        Console.WriteLine(
            $"  [{tier.Name}] tenant {grand.tenants} | {petIds.Count} pets, {reservationsTarget} reservas, {healthCount} fichas | {sw.Elapsed.TotalSeconds:F1}s");
    }
}

var manifest = new
{
    generatedAt = DateTimeOffset.UtcNow,
    issuer,
    audience,
    tenants = manifestTenants,
};

var fullOutput = Path.GetFullPath(outputPath);
Directory.CreateDirectory(Path.GetDirectoryName(fullOutput)!);
await File.WriteAllTextAsync(fullOutput, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine();
Console.WriteLine($"OK em {totalSw.Elapsed.TotalSeconds:F1}s | tenants={grand.tenants} tutores={grand.tutors} pets={grand.pets} acomodações={grand.accommodations} reservas={grand.reservations} fichas={grand.health}");
Console.WriteLine($"Manifesto: {fullOutput}");
return;

// --- helpers ---
int Scaled(int n) => Math.Max(1, (int)Math.Round(n * scale));

static async Task FlushAsync<T>(DbContext db, List<T> batch) where T : class
{
    if (batch.Count == 0)
    {
        return;
    }

    db.AddRange(batch);
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear(); // mantém o change tracker pequeno em lotes grandes
    batch.Clear();
}

static string Mask(string connectionString) =>
    System.Text.RegularExpressions.Regex.Replace(connectionString, "(?i)(password)=[^;]*", "$1=***");

internal sealed record Tier(string Name, int Count, int Tutors, int Accommodations, int Reservations, double HealthFraction);
