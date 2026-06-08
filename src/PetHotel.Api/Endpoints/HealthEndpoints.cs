using PetHotel.Api.Http;
using PetHotel.Health.Application.HealthRecords;
using PetHotel.Health.Application.HealthRecords.GetPetHealth;
using PetHotel.Health.Application.Vaccinations.RegisterVaccination;
using PetHotel.Health.Domain.HealthRecords;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Health: registro de vacinação e consulta da ficha/aptidão.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>Corpo do registro de vacinação (o pet vem da rota).</summary>
    public sealed record RegisterVaccinationRequest(VaccineType Type, DateOnly AppliedOn, DateOnly ExpiresOn);

    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/pets/{petId:guid}").WithTags("Health").RequireAuthorization();

        group.MapPost("/vaccinations",
                async (Guid petId, RegisterVaccinationRequest body, IMessageBus bus, CancellationToken ct) =>
                {
                    var command = new RegisterVaccination(petId, body.Type, body.AppliedOn, body.ExpiresOn);
                    var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                    return result.ToHttpResult(id => Results.Created($"/v1/pets/{petId}/vaccinations/{id}", new CreatedResponse(id)));
                })
            .WithName("RegisterVaccination")
            .WithSummary("Registra uma vacinação na ficha do pet.")
            .WithDescription("Cria a ficha de saúde se ainda não existir. Requer tenant no contexto.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/health", async (Guid petId, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<PetHealthDto>>(new GetPetHealth(petId), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetPetHealth")
            .WithSummary("Consulta a ficha de saúde do pet e a aptidão sanitária (clearance) de hoje.")
            .Produces<PetHealthDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
