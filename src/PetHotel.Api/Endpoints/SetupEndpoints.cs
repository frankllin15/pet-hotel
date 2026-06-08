using PetHotel.Api.Http;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Configuration;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>Wizard de setup do hotel (config operacional). Restrito a admin do tenant.</summary>
public static class SetupEndpoints
{
    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/setup").WithTags("Setup").RequireAuthorization("TenantAdmin");

        group.MapGet("/configuration", async (IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<TenantConfigurationDto>>(new GetTenantConfiguration(), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetTenantConfiguration")
            .WithSummary("Consulta a configuração operacional do hotel.")
            .Produces<TenantConfigurationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/configuration", async (UpdateTenantConfiguration command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(command, ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("UpdateTenantConfiguration")
            .WithSummary("Define tipos de acomodação, vacinas obrigatórias e horários. Conclui o setup.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
