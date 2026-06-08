using PetHotel.Api.Http;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Tenants;
using PetHotel.Tenancy.Application.Tenants.GetTenantById;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Tenancy. Criação de tenant e usuários NÃO fica aqui: tenant é
/// provisionado (ProvisioningEndpoints) e usuários entram por convite (auth/invites).
/// </summary>
public static class TenancyEndpoints
{
    public static IEndpointRouteBuilder MapTenancyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Tenancy");

        group.MapGet("/tenants/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<TenantDto>>(new GetTenantById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetTenantById")
            .WithSummary("Busca um hotel por Id.")
            .RequireAuthorization()
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
