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
                return result.ToHttpResult(id => Results.Created($"/v1/tenants/{id}", new CreatedResponse(id)));
            })
            .WithName("RegisterTenant")
            .WithSummary("Registra um novo hotel (tenant).")
            .WithDescription("Operação de nível de plataforma. O slug deve ser único e em kebab-case.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/tenants/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<TenantDto>>(new GetTenantById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetTenantById")
            .WithSummary("Busca um hotel por Id.")
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/users", async (RegisterUser command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/users/{id}", new CreatedResponse(id)));
            })
            .WithName("RegisterUser")
            .WithSummary("Registra um usuário no tenant corrente.")
            .WithDescription("O tenant vem do token (claim tenant_id), nunca do payload. Sem tenant no contexto: 403.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
