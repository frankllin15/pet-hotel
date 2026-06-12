using Microsoft.Extensions.Options;
using PetHotel.Api.Http;
using PetHotel.Api.Storage;
using PetHotel.Health.Application.HealthRecords;
using PetHotel.Health.Application.HealthRecords.GetPetHealth;
using PetHotel.Health.Application.ParasiteTreatments.RegisterParasiteTreatment;
using PetHotel.Health.Application.ParasiteTreatments.UpdateParasiteTreatment;
using PetHotel.Health.Application.Vaccinations.RegisterVaccination;
using PetHotel.Health.Application.Vaccinations.SetVaccinationPhoto;
using PetHotel.Health.Application.Vaccinations.UpdateVaccination;
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

    /// <summary>Resposta do upload de foto da carteira: URL relativa para baixar.</summary>
    public sealed record VaccinationPhotoResponse(string PhotoUrl);

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

        group.MapPut("/vaccinations/{vaccinationId:guid}",
                async (Guid petId, Guid vaccinationId, RegisterVaccinationRequest body, IMessageBus bus, CancellationToken ct) =>
                {
                    var command = new UpdateVaccination(petId, vaccinationId, body.Type, body.AppliedOn, body.ExpiresOn);
                    var result = await bus.InvokeAsync<Result>(command, ct);
                    return result.ToHttpResult(Results.NoContent());
                })
            .WithName("UpdateVaccination")
            .WithSummary("Edita uma vacinação existente na ficha do pet.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

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

        group.MapPut("/parasite-treatments/{treatmentId:guid}",
                async (Guid petId, Guid treatmentId, RegisterParasiteTreatmentRequest body, IMessageBus bus, CancellationToken ct) =>
                {
                    var command = new UpdateParasiteTreatment(
                        petId, treatmentId, body.Type, body.ProductName, body.AppliedOn, body.NextDueOn);
                    var result = await bus.InvokeAsync<Result>(command, ct);
                    return result.ToHttpResult(Results.NoContent());
                })
            .WithName("UpdateParasiteTreatment")
            .WithSummary("Edita um controle de parasitas existente na ficha do pet.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

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

        group.MapPost("/vaccinations/{vaccinationId:guid}/photo", async (
                Guid petId, Guid vaccinationId, IFormFile file, IFileStorage storage,
                IOptions<FileStorageOptions> options, IMessageBus bus, CancellationToken ct) =>
            {
                var saved = await ImageUploads.SaveAsync(file, "vaccinations", storage, options, ct);
                if (saved.IsFailure)
                {
                    return saved.ToHttpResult(_ => Results.Empty);
                }

                var key = saved.Value.Key;
                var setResult = await bus.InvokeAsync<Result<string?>>(
                    new SetVaccinationPhoto(petId, vaccinationId, key), ct);
                if (setResult.IsFailure)
                {
                    await storage.DeleteAsync(key, ct); // não deixa arquivo órfão
                    return setResult.ToHttpResult(_ => Results.Empty);
                }

                if (setResult.Value is { } previous && previous != key)
                {
                    await storage.DeleteAsync(previous, ct);
                }

                return Results.Ok(new VaccinationPhotoResponse($"/v1/files/{key}"));
            })
            .DisableAntiforgery()
            .WithName("SetVaccinationPhoto")
            .WithSummary("Envia (ou substitui) a foto da carteira para uma vacinação.")
            .WithDescription("Multipart com o campo 'file'. JPEG, PNG ou WebP até o limite configurado.")
            .Produces<VaccinationPhotoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/vaccinations/{vaccinationId:guid}/photo", async (
                Guid petId, Guid vaccinationId, IFileStorage storage, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<string?>>(
                    new SetVaccinationPhoto(petId, vaccinationId, null), ct);
                if (result.IsSuccess && result.Value is { } previous)
                {
                    await storage.DeleteAsync(previous, ct);
                }

                return result.ToHttpResult(_ => Results.NoContent());
            })
            .WithName("RemoveVaccinationPhoto")
            .WithSummary("Remove a foto da carteira de uma vacinação.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
