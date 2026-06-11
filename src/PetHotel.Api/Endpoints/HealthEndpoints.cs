using PetHotel.Api.Http;
using PetHotel.Health.Application.HealthRecords;
using PetHotel.Health.Application.HealthRecords.GetPetHealth;
using PetHotel.Health.Application.ParasiteTreatments.RegisterParasiteTreatment;
using PetHotel.Health.Application.Vaccinations.RegisterVaccination;
using PetHotel.Health.Application.VetContacts.SetVetContact;
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

    /// <summary>Corpo do registro de controle de parasitas (o pet vem da rota).</summary>
    public sealed record RegisterParasiteTreatmentRequest(
        ParasiteTreatmentType Type, string? ProductName, DateOnly AppliedOn, DateOnly? NextDueOn);

    /// <summary>Corpo da definição do veterinário particular (o pet vem da rota).</summary>
    public sealed record SetVetContactRequest(string Name, string Phone, string? Clinic);

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

        group.MapPost("/parasite-treatments",
                async (Guid petId, RegisterParasiteTreatmentRequest body, IMessageBus bus, CancellationToken ct) =>
                {
                    var command = new RegisterParasiteTreatment(petId, body.Type, body.ProductName, body.AppliedOn, body.NextDueOn);
                    var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                    return result.ToHttpResult(id => Results.Created($"/v1/pets/{petId}/parasite-treatments/{id}", new CreatedResponse(id)));
                })
            .WithName("RegisterParasiteTreatment")
            .WithSummary("Registra um controle de parasitas (antipulgas/vermífugo) na ficha do pet.")
            .WithDescription("Cria a ficha de saúde se ainda não existir. Requer tenant no contexto.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/vet-contact",
                async (Guid petId, SetVetContactRequest body, IMessageBus bus, CancellationToken ct) =>
                {
                    var command = new SetVetContact(petId, body.Name, body.Phone, body.Clinic);
                    var result = await bus.InvokeAsync<Result>(command, ct);
                    return result.ToHttpResult(Results.NoContent());
                })
            .WithName("SetVetContact")
            .WithSummary("Define (ou substitui) o veterinário particular na ficha do pet.")
            .WithDescription("Cria a ficha de saúde se ainda não existir. Requer tenant no contexto.")
            .Produces(StatusCodes.Status204NoContent)
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
