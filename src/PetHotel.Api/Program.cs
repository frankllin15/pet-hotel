using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetHotel.Api.Adapters;
using PetHotel.Api.Endpoints;
using PetHotel.BuildingBlocks.Multitenancy;
using PetHotel.SharedKernel;
using PetHotel.Booking.Infrastructure;
using PetHotel.Health.Infrastructure;
using PetHotel.Registry.Infrastructure;
using PetHotel.Tenancy.Infrastructure;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");

// --- Logging estruturado (docs/05) ---
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// --- Contexto transversal (tenant + usuário, docs/04) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton<ITenantConnectionResolver>(new SharedTenantConnectionResolver(connectionString));

// --- Erros padronizados (ProblemDetails, RFC 9457, docs/02) ---
builder.Services.AddProblemDetails();

// Enums trafegam como string no JSON (entrada e saída).
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Módulos (cada um registra suas portas/adaptadores, docs/02) ---
builder.Services.AddTenancyModule(connectionString);
builder.Services.AddRegistryModule(connectionString);
builder.Services.AddHealthModule(connectionString);
builder.Services.AddBookingModule(connectionString);

// --- Wolverine: mediator + Outbox durável no Postgres (docs/05) ---
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(connectionString);
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();

    // Descoberta de handlers nos assemblies de Application de cada módulo.
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Tenancy.Application.AssemblyReference).Assembly);
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Registry.Application.AssemblyReference).Assembly);
});

// --- Observabilidade (OpenTelemetry, docs/05) ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PetHotel.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// --- Health checks: liveness vs readiness (docs/05) ---
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

// Liveness: o processo está de pé.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
// Readiness: dependências (banco, Outbox) prontas — checks marcados com a tag "ready".
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// --- Endpoints por módulo (docs/02) ---
app.MapTenancyEndpoints();
app.MapRegistryEndpoints();

app.Run();

// Exposto para os testes de integração (WebApplicationFactory).
public partial class Program;
