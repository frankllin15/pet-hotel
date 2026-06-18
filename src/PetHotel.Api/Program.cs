using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetHotel.Api.Adapters;
using PetHotel.Api.Endpoints;
using PetHotel.Api.Observability;
using PetHotel.Api.Storage;
using PetHotel.BuildingBlocks.Multitenancy;
using PetHotel.SharedKernel;
using PetHotel.Booking.Infrastructure;
using PetHotel.Health.Infrastructure;
using PetHotel.Notifications.Infrastructure;
using PetHotel.Operations.Infrastructure;
using PetHotel.Registry.Infrastructure;
using PetHotel.Tenancy.Infrastructure;
using PetHotel.Tenancy.Infrastructure.Auth;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

// --- Sentry: rastreamento de erro de ponta a ponta (docs/10) ---
// Lê a seção "Sentry" do appsettings. Sem 'Sentry:Dsn' (default), o SDK fica
// desligado (no-op) — mesmo "gate" do front. Captura exceções não tratadas;
// erros de negócio usam Result<T> (não viram exceção), então não geram ruído.
builder.WebHost.UseSentry();

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");

// --- Pool de conexões único e compartilhado (docs/04) ---
// Um NpgsqlDataSource = um pool. Compartilhado por todos os DbContexts e pelo
// Wolverine para que 'Maximum Pool Size' seja o teto real do processo. Sem isso,
// cada UseNpgsql(connectionString) cria um data source próprio (= pool próprio) e o
// limite vira N × MaxPoolSize, estourando o max_connections do Postgres sob carga.
var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
builder.Services.AddSingleton(dataSource);

// --- Logging estruturado (docs/05) ---
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();

    // Com um coletor OTLP de pé (OTEL_EXPORTER_OTLP_ENDPOINT definido, ex.: o Aspire
    // Dashboard em dev), os logs também vão por OTLP — assim aparecem no painel
    // correlacionados ao trace pelo trace id / correlation id (docs/10). Sem o
    // endpoint (prod sem coletor), só o Console.
    var otlpEndpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        loggerConfig.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = otlpEndpoint;
            options.Protocol = OtlpProtocol.Grpc;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = "PetHotel.Api"
            };
        });
    }
});

// --- Contexto transversal (tenant + usuário, docs/04) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton<ITenantConnectionResolver>(new SharedTenantConnectionResolver(connectionString));

// --- Storage de arquivos (fotos), tenant-scoped (docs/04) ---
builder.Services.AddFileStorage(builder.Configuration);

// --- Erros padronizados (ProblemDetails, RFC 9457, docs/02) ---
builder.Services.AddProblemDetails();

// --- Autenticação/Autorização (JWT, memória onboarding-and-auth) ---
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Seção 'Jwt' não configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("TenantAdmin", policy => policy.RequireRole("Owner", "Manager")));

// --- Documentação da API (OpenAPI) ---
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "PetHotel API";
        document.Info.Version = "v1";
        document.Info.Description =
            "API do Hotel de Pets. Monólito modular (DDD + Hexagonal). " +
            "Operações tenant-scoped exigem o tenant resolvido do token de autenticação.";

        // Esquema Bearer para o botão Authorize do Swagger.
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Cole o JWT do /v1/auth/login (sem o prefixo 'Bearer ')."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

// Enums trafegam como string no JSON (entrada e saída).
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Módulos (cada um registra suas portas/adaptadores, docs/02) ---
builder.Services.AddTenancyModule(dataSource);
builder.Services.AddRegistryModule(dataSource);
builder.Services.AddHealthModule(dataSource);
builder.Services.AddBookingModule(dataSource);
builder.Services.AddOperationsModule(dataSource);
builder.Services.AddNotificationsModule(dataSource);

// --- Wolverine: mediator + Outbox durável no Postgres (docs/05) ---
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithPostgresql(dataSource);
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.AutoApplyTransactions();
    opts.Policies.UseDurableLocalQueues();

    // Descoberta de handlers nos assemblies de Application de cada módulo.
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Tenancy.Application.AssemblyReference).Assembly);
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Registry.Application.AssemblyReference).Assembly);
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Health.Application.AssemblyReference).Assembly);
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Booking.Application.AssemblyReference).Assembly);
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Operations.Application.AssemblyReference).Assembly);
    opts.Discovery.IncludeAssembly(typeof(PetHotel.Notifications.Application.AssemblyReference).Assembly);
});

// --- Observabilidade (OpenTelemetry, docs/05) ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PetHotel.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("Wolverine") // spans dos handlers e da mensageria/Outbox (docs/05)
        .AddSource("Npgsql")    // spans das queries no Postgres (visibilidade do banco)
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// --- Health checks: liveness vs readiness (docs/05) ---
// Readiness checa o Postgres (e, por tabela, o Outbox do Wolverine) — tag "ready".
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

var app = builder.Build();

// Antes do request logging: o correlation id enriquece a linha de log da request
// (e todo log emitido durante ela), o trace e o escopo do Sentry.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

// OpenAPI + Swagger UI apenas em desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PetHotel API v1"));
}

// Liveness: o processo está de pé.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
// Readiness: dependências (banco, Outbox) prontas — checks marcados com a tag "ready".
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// --- Endpoints por módulo (docs/02) ---
app.MapProvisioningEndpoints();
app.MapAuthEndpoints();
app.MapInvitationsEndpoints();
app.MapUsersEndpoints();
app.MapSetupEndpoints();
app.MapTenancyEndpoints();
app.MapRegistryEndpoints();
app.MapHealthEndpoints();
app.MapBookingEndpoints();
app.MapOperationsEndpoints();
app.MapNotificationsEndpoints();
app.MapDashboardEndpoints();
app.MapFilesEndpoints();

app.Run();

// Exposto para os testes de integração (WebApplicationFactory).
public partial class Program;
