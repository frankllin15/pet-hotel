using Microsoft.Extensions.Options;
using PetHotel.Api.Http;
using PetHotel.Api.Storage;
using PetHotel.Registry.Application.Packs;
using PetHotel.Registry.Application.Packs.CreatePack;
using PetHotel.Registry.Application.Packs.DeletePack;
using PetHotel.Registry.Application.Packs.GetPackById;
using PetHotel.Registry.Application.Packs.ListPacks;
using PetHotel.Registry.Application.Packs.UpdatePack;
using PetHotel.Registry.Application.Pets;
using PetHotel.Registry.Application.Pets.DeletePet;
using PetHotel.Registry.Application.Pets.SetPetPhoto;
using PetHotel.Registry.Application.Pets.GetPetById;
using PetHotel.Registry.Application.Pets.ListPets;
using PetHotel.Registry.Application.Pets.RegisterPet;
using PetHotel.Registry.Application.Pets.UpdatePet;
using PetHotel.Registry.Application.Tutors;
using PetHotel.Registry.Application.Tutors.DeleteTutor;
using PetHotel.Registry.Application.Tutors.GetTutorById;
using PetHotel.Registry.Application.Tutors.ListTutors;
using PetHotel.Registry.Application.Tutors.RegisterTutor;
using PetHotel.Registry.Application.Tutors.SetTutorConsents;
using PetHotel.Registry.Application.Tutors.UpdateTutor;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Registry. Recebem o request, enviam Command/Query via
/// Wolverine e mapeiam o Result para HTTP (docs/02).
/// </summary>
public static class RegistryEndpoints
{
    public static IEndpointRouteBuilder MapRegistryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Registry").RequireAuthorization();

        group.MapPost("/tutors", async (RegisterTutor command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/tutors/{id}", new CreatedResponse(id)));
            })
            .WithName("RegisterTutor")
            .WithSummary("Cadastra um tutor no tenant corrente.")
            .WithDescription("Requer tenant no contexto (token). E-mail único por hotel.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/tutors", async (
                string? search, string? cursor, int? limit, IMessageBus bus, CancellationToken ct) =>
            {
                var query = new ListTutors(search, cursor, limit ?? 20);
                var result = await bus.InvokeAsync<Result<CursorPage<TutorDto>>>(query, ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("ListTutors")
            .WithSummary("Lista tutores do tenant corrente (paginado por cursor).")
            .WithDescription("Filtro opcional por nome (search). Use o nextCursor para a próxima página.")
            .Produces<CursorPage<TutorDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/tutors/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<TutorDto>>(new GetTutorById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetTutorById")
            .WithSummary("Busca um tutor por Id no tenant corrente.")
            .Produces<TutorDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/tutors/{id:guid}", async (Guid id, UpdateTutor command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(command with { Id = id }, ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("UpdateTutor")
            .WithSummary("Edita um tutor existente no tenant corrente.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/tutors/{id:guid}/consents", async (Guid id, SetTutorConsents command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(command with { TutorId = id }, ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("SetTutorConsents")
            .WithSummary("Registra os consentimentos LGPD do tutor.")
            .WithDescription("Uso de imagem, marketing e compartilhamento. Carimba data e versão dos termos; decisão inalterada preserva o registro.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/tutors/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new DeleteTutor(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("DeleteTutor")
            .WithSummary("Exclui um tutor do tenant corrente.")
            .WithDescription("Bloqueado (409) se o tutor ainda tiver pets vinculados.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/pets", async (RegisterPet command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/pets/{id}", new CreatedResponse(id)));
            })
            .WithName("RegisterPet")
            .WithSummary("Cadastra um pet para um tutor existente.")
            .WithDescription("O tutor precisa existir no tenant corrente; caso contrário, 404.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/pets", async (
                string? search, Guid? tutorId, string? cursor, int? limit, IMessageBus bus, CancellationToken ct) =>
            {
                var query = new ListPets(search, tutorId, cursor, limit ?? 20);
                var result = await bus.InvokeAsync<Result<CursorPage<PetDto>>>(query, ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("ListPets")
            .WithSummary("Lista pets do tenant corrente (paginado por cursor).")
            .WithDescription("Filtros opcionais por nome (search) e tutorId. Use o nextCursor para a próxima página.")
            .Produces<CursorPage<PetDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/pets/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<PetDto>>(new GetPetById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetPetById")
            .WithSummary("Busca um pet por Id no tenant corrente.")
            .Produces<PetDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/pets/{id:guid}", async (Guid id, UpdatePet command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(command with { Id = id }, ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("UpdatePet")
            .WithSummary("Edita um pet existente no tenant corrente.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/pets/{id:guid}/photo", async (
                Guid id, IFormFile file, IFileStorage storage, IOptions<FileStorageOptions> options,
                IMessageBus bus, CancellationToken ct) =>
            {
                var saved = await ImageUploads.SaveAsync(file, "pets", storage, options, ct);
                if (saved.IsFailure)
                {
                    return saved.ToHttpResult(_ => Results.Empty);
                }

                var key = saved.Value.Key;
                var setResult = await bus.InvokeAsync<Result<string?>>(new SetPetPhoto(id, key), ct);
                if (setResult.IsFailure)
                {
                    await storage.DeleteAsync(key, ct); // não deixa arquivo órfão se o pet não existir
                    return setResult.ToHttpResult(_ => Results.Empty);
                }

                // Substituiu uma foto anterior: apaga a antiga.
                if (setResult.Value is { } previous && previous != key)
                {
                    await storage.DeleteAsync(previous, ct);
                }

                return Results.Ok(new PetPhotoResponse($"/v1/files/{key}"));
            })
            .DisableAntiforgery()
            .WithName("SetPetPhoto")
            .WithSummary("Envia (ou substitui) a foto do pet.")
            .WithDescription("Multipart com o campo 'file'. JPEG, PNG ou WebP até o limite configurado.")
            .Produces<PetPhotoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/pets/{id:guid}/photo", async (Guid id, IFileStorage storage, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<string?>>(new SetPetPhoto(id, null), ct);
                if (result.IsSuccess && result.Value is { } previous)
                {
                    await storage.DeleteAsync(previous, ct);
                }

                return result.ToHttpResult(_ => Results.NoContent());
            })
            .WithName("RemovePetPhoto")
            .WithSummary("Remove a foto do pet.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/pets/{id:guid}", async (Guid id, IFileStorage storage, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<string?>>(new DeletePet(id), ct);
                if (result.IsSuccess && result.Value is { } photoKey)
                {
                    await storage.DeleteAsync(photoKey, ct); // limpa o arquivo da foto, se houver
                }

                return result.ToHttpResult(_ => Results.NoContent());
            })
            .WithName("DeletePet")
            .WithSummary("Exclui um pet do tenant corrente (e a foto associada).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // --- Matilhas (grupos de pets) ---
        group.MapPost("/packs", async (CreatePack command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/packs/{id}", new CreatedResponse(id)));
            })
            .WithName("CreatePack")
            .WithSummary("Cria uma matilha (grupo de pets compatíveis).")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/packs", async (IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<PackSummaryDto>>>(new ListPacks(), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("ListPacks")
            .WithSummary("Lista as matilhas do tenant corrente.")
            .Produces<IReadOnlyList<PackSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/packs/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<PackDto>>(new GetPackById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetPackById")
            .WithSummary("Busca uma matilha por Id (com membros e alertas de compatibilidade).")
            .Produces<PackDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/packs/{id:guid}", async (Guid id, UpdatePack command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(command with { Id = id }, ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("UpdatePack")
            .WithSummary("Edita uma matilha (nome, observações e composição).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/packs/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new DeletePack(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("DeletePack")
            .WithSummary("Exclui uma matilha do tenant corrente.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>Resposta do upload de foto: URL relativa para baixar pelo endpoint de arquivos.</summary>
    public sealed record PetPhotoResponse(string PhotoUrl);
}
