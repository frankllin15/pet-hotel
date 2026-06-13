using PetHotel.Api.Http;
using PetHotel.Notifications.Application.Reports;
using PetHotel.Notifications.Application.Reports.CreateReport;
using PetHotel.Notifications.Application.Reports.GetStayReports;
using PetHotel.Notifications.Application.Reports.GetTutorReports;
using PetHotel.Notifications.Application.Reports.SendReport;
using PetHotel.SharedKernel;
using Wolverine;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Notifications: relatórios diários ao tutor (montados a partir do diário),
/// marcar como enviado e histórico (por estadia e por tutor). Restritos a usuários autenticados.
/// </summary>
public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1").WithTags("Notifications").RequireAuthorization();

        group.MapPost("/reports", async (CreateReport command, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
                return result.ToHttpResult(id => Results.Created($"/v1/reports/{id}", new CreatedResponse(id)));
            })
            .WithName("CreateReport")
            .WithSummary("Cria (rascunho) um relatório ao tutor a partir do diário de uma estadia/dia.")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/reports/{id:guid}/send", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result>(new SendReport(id), ct);
                return result.ToHttpResult(Results.NoContent());
            })
            .WithName("SendReport")
            .WithSummary("Marca um relatório como enviado/compartilhado.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/reservations/{reservationId:guid}/reports", async (
                Guid reservationId, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<OutboundMessageDto>>>(
                    new GetStayReports(reservationId), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetStayReports")
            .WithSummary("Histórico de relatórios de uma estadia.")
            .Produces<IReadOnlyList<OutboundMessageDto>>(StatusCodes.Status200OK);

        group.MapGet("/tutors/{tutorId:guid}/reports", async (
                Guid tutorId, IMessageBus bus, CancellationToken ct) =>
            {
                var result = await bus.InvokeAsync<Result<IReadOnlyList<OutboundMessageDto>>>(
                    new GetTutorReports(tutorId), ct);
                return result.ToHttpResult(Results.Ok);
            })
            .WithName("GetTutorReports")
            .WithSummary("Histórico de relatórios enviados a um tutor.")
            .Produces<IReadOnlyList<OutboundMessageDto>>(StatusCodes.Status200OK);

        return app;
    }
}
