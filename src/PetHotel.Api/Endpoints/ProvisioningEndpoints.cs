using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PetHotel.Api.Http;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Provisioning;
using PetHotel.Tenancy.Domain.Ports;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Provisionamento de tenant (operação de plataforma, rara). Protegida por chave de
/// plataforma (header X-Platform-Key), não por JWT de tenant (memória onboarding-and-auth).
/// Invoca o handler direto via DI (toca Identity/UserManager — fora do codegen do Wolverine).
/// </summary>
public static class ProvisioningEndpoints
{
    public static IEndpointRouteBuilder MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/provisioning/tenants",
                async (
                    ProvisionTenant command,
                    [FromHeader(Name = "X-Platform-Key")] string? platformKey,
                    IConfiguration config,
                    IValidator<ProvisionTenant> validator,
                    ITenantRepository tenants,
                    ITenantConfigurationRepository configurations,
                    IUserAccountService userAccounts,
                    TimeProvider clock,
                    CancellationToken ct) =>
                {
                    var expected = config["PlatformKey"];
                    if (string.IsNullOrEmpty(expected) || !string.Equals(platformKey, expected, StringComparison.Ordinal))
                    {
                        return Results.Problem(
                            detail: "Chave de plataforma inválida.",
                            statusCode: StatusCodes.Status401Unauthorized,
                            title: "platform.unauthorized");
                    }

                    var result = await ProvisionTenantHandler.Handle(
                        command, validator, tenants, configurations, userAccounts, clock, ct);

                    // ActivationToken volta no corpo só no MVP; em produção iria por e-mail (Notifications).
                    return result.ToHttpResult(p => Results.Created($"/v1/tenants/{p.TenantId}", p));
                })
            .WithTags("Provisioning")
            .WithName("ProvisionTenant")
            .WithSummary("Provisiona um novo hotel (tenant + admin pendente + config default).")
            .AllowAnonymous()
            .Produces<ProvisionedTenant>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
