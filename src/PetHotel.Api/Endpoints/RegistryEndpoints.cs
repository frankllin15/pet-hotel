using PetHotel.Api.Http;
using PetHotel.Registry.Application.Pets;
using PetHotel.Registry.Application.Pets.GetPetById;
using PetHotel.Registry.Application.Pets.RegisterPet;
using PetHotel.Registry.Application.Tutors;
using PetHotel.Registry.Application.Tutors.GetTutorById;
using PetHotel.Registry.Application.Tutors.RegisterTutor;
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

        group.MapGet("/tutors/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<TutorDto>>(new GetTutorById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetTutorById")
            .WithSummary("Busca um tutor por Id no tenant corrente.")
            .Produces<TutorDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

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

        group.MapGet("/pets/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<PetDto>>(new GetPetById(id), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetPetById")
            .WithSummary("Busca um pet por Id no tenant corrente.")
            .Produces<PetDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
