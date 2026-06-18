using FluentValidation;
using Microsoft.Extensions.Options;
using PetHotel.Api.Http;
using PetHotel.Api.Storage;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.CareLog;
using PetHotel.Operations.Application.CareLog.AddCareEntryPhoto;
using PetHotel.Operations.Application.CareLog.GetStayCareLog;
using PetHotel.Operations.Application.CareLog.LogCareEntry;
using PetHotel.Operations.Application.CareLog.RemoveCareEntryPhoto;
using PetHotel.Operations.Application.Incidents;
using PetHotel.Operations.Application.Incidents.GetStayIncidents;
using PetHotel.Operations.Application.Incidents.ReportIncident;
using PetHotel.Operations.Application.Medications;
using PetHotel.Operations.Application.Medications.GetStayMedications;
using PetHotel.Operations.Application.Medications.RecordMedication;
using PetHotel.Operations.Application.Tasks;
using PetHotel.Operations.Application.Tasks.CreateTask;
using PetHotel.Operations.Application.Tasks.DeleteTask;
using PetHotel.Operations.Application.Tasks.ListTasks;
using PetHotel.Operations.Application.Tasks.SetTaskDone;
using PetHotel.Operations.Application.Tasks.UpdateTask;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Operations: diário de bordo, fotos da ocorrência, log de medicação e
/// incidentes — tudo por estadia (reserva). Registrar exige check-in feito.
/// </summary>
public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Operations").RequireAuthorization();

        // --- Diário de bordo (cross-módulo: Operations grava + Booking lê → direto via DI) ---
        group.MapPost("/reservations/{reservationId:guid}/care-log", async (
                Guid reservationId, LogCareEntry command,
                IValidator<LogCareEntry> validator, ITenantContext tenantContext, IStayGateway stays,
                ICareLogRepository entries, IUnitOfWork unitOfWork, TimeProvider clock, CancellationToken ct) =>
            {
                var result = await LogCareEntryHandler.Handle(
                    command with { ReservationId = reservationId },
                    validator, tenantContext, stays, entries, unitOfWork, clock, ct);
                return result.ToHttpResult(id =>
                    Results.Created($"/v1/reservations/{reservationId}/care-log/{id}", new CreatedResponse(id)));
            })
            .WithName("LogCareEntry")
            .WithSummary("Registra uma ocorrência no diário de bordo da estadia.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/reservations/{reservationId:guid}/care-log", async (
                Guid reservationId, string? cursor, int? limit, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<CursorPage<CareLogEntryDto>>>(
                    new GetStayCareLog(reservationId, cursor, limit ?? 20), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetStayCareLog")
            .WithSummary("Timeline do diário de bordo da estadia (paginada por cursor).")
            .Produces<CursorPage<CareLogEntryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // --- Fotos da ocorrência (toca só Operations → via bus) ---
        group.MapPost("/care-log/{entryId:guid}/photos", async (
                Guid entryId, IFormFile file, IFileStorage storage, IOptions<FileStorageOptions> options,
                IMessageBus bus, CancellationToken ct) =>
            {
                var saved = await ImageUploads.SaveAsync(file, "care-log", storage, options, ct);
                if (saved.IsFailure)
                {
                    return saved.ToHttpResult(_ => Results.Empty);
                }

                var key = saved.Value.Key;
                var result = await bus.InvokeAsync<Result>(new AddCareEntryPhoto(entryId, key), ct);
                if (result.IsFailure)
                {
                    await storage.DeleteAsync(key, ct);
                    return result.ToHttpResult(Results.Empty);
                }

                return Results.Ok(new CarePhotoResponse($"/v1/files/{key}"));
            })
            .DisableAntiforgery()
            .WithName("AddCareEntryPhoto")
            .WithSummary("Anexa uma foto a uma ocorrência do diário.")
            .Produces<CarePhotoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/care-log/{entryId:guid}/photos", async (
                Guid entryId, string key, IFileStorage storage, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new RemoveCareEntryPhoto(entryId, key), ct);
                if (result.IsSuccess)
                {
                    await storage.DeleteAsync(key, ct);
                }

                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("RemoveCareEntryPhoto")
            .WithSummary("Remove uma foto de uma ocorrência do diário (chave via query).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // --- Medicação (cross-módulo → direto via DI) ---
        group.MapPost("/reservations/{reservationId:guid}/medications", async (
                Guid reservationId, RecordMedication command,
                IValidator<RecordMedication> validator, ITenantContext tenantContext, IStayGateway stays,
                IMedicationRepository medications, IUnitOfWork unitOfWork, TimeProvider clock, CancellationToken ct) =>
            {
                var result = await RecordMedicationHandler.Handle(
                    command with { ReservationId = reservationId },
                    validator, tenantContext, stays, medications, unitOfWork, clock, ct);
                return result.ToHttpResult(id =>
                    Results.Created($"/v1/reservations/{reservationId}/medications/{id}", new CreatedResponse(id)));
            })
            .WithName("RecordMedication")
            .WithSummary("Registra a administração de um medicamento na estadia (quem/quando/dose).")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/reservations/{reservationId:guid}/medications", async (
                Guid reservationId, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<MedicationDto>>>(
                    new GetStayMedications(reservationId), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetStayMedications")
            .WithSummary("Medicações registradas na estadia.")
            .Produces<IReadOnlyList<MedicationDto>>(StatusCodes.Status200OK);

        // --- Incidentes (cross-módulo → direto via DI) ---
        group.MapPost("/reservations/{reservationId:guid}/incidents", async (
                Guid reservationId, ReportIncident command,
                IValidator<ReportIncident> validator, ITenantContext tenantContext, IStayGateway stays,
                IIncidentRepository incidents, IUnitOfWork unitOfWork, TimeProvider clock, CancellationToken ct) =>
            {
                var result = await ReportIncidentHandler.Handle(
                    command with { ReservationId = reservationId },
                    validator, tenantContext, stays, incidents, unitOfWork, clock, ct);
                return result.ToHttpResult(id =>
                    Results.Created($"/v1/reservations/{reservationId}/incidents/{id}", new CreatedResponse(id)));
            })
            .WithName("ReportIncident")
            .WithSummary("Registra um incidente na estadia (auditável).")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/reservations/{reservationId:guid}/incidents", async (
                Guid reservationId, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<IncidentDto>>>(
                    new GetStayIncidents(reservationId), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetStayIncidents")
            .WithSummary("Incidentes registrados na estadia.")
            .Produces<IReadOnlyList<IncidentDto>>(StatusCodes.Status200OK);

        // --- Tarefas operacionais do dia (por tenant, não por estadia; contexto único → via bus) ---
        group.MapGet("/tasks", async (DateOnly date, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<OperationalTaskDto>>>(new ListTasks(date), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("ListTasks")
            .WithSummary("Tarefas operacionais de um dia (parâmetro 'date').")
            .Produces<IReadOnlyList<OperationalTaskDto>>(StatusCodes.Status200OK);

        group.MapPost("/tasks", async (CreateTask command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/tasks/{id}", new CreatedResponse(id)));
            })
            .WithName("CreateTask")
            .WithSummary("Cria uma tarefa operacional para um dia.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/tasks/{id:guid}", async (Guid id, UpdateTask command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(command with { Id = id }, ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("UpdateTask")
            .WithSummary("Edita título, categoria e responsável de uma tarefa.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/tasks/{id:guid}/done", async (Guid id, SetTaskDoneRequest body, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new SetTaskDone(id, body.Done), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("SetTaskDone")
            .WithSummary("Marca a tarefa como feita/não-feita.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/tasks/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new DeleteTask(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("DeleteTask")
            .WithSummary("Exclui uma tarefa operacional.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>Resposta do upload de foto de ocorrência: URL relativa para o endpoint de arquivos.</summary>
    public sealed record CarePhotoResponse(string PhotoUrl);

    /// <summary>Corpo do toggle de conclusão de tarefa.</summary>
    public sealed record SetTaskDoneRequest(bool Done);
}
