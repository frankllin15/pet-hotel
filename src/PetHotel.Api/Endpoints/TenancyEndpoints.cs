using PetHotel.Api.Http;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Tenants;
using PetHotel.Tenancy.Application.Tenants.GetTenantById;
using PetHotel.Tenancy.Application.Tenants.RegisterTenant;
using PetHotel.Tenancy.Application.Users.RegisterUser;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Tenancy. Não contêm regra: recebem o request, enviam o
/// Command/Query via Wolverine e mapeiam o Result para HTTP (docs/02).
/// </summary>
public static class TenancyEndpoints
{
    public static IEndpointRouteBuilder MapTenancyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Tenancy");

        group.MapPost("/tenants", async (RegisterTenant command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToHttpResult(id => Results.Created($"/v1/tenants/{id}", new { id }));
        });

        group.MapGet("/tenants/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<TenantDto>>(new GetTenantById(id), ct);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapPost("/users", async (RegisterUser command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToHttpResult(id => Results.Created($"/v1/users/{id}", new { id }));
        });

        return app;
    }
}
