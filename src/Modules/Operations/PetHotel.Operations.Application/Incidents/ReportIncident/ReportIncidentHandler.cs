using FluentValidation;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Validation;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Incidents;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;
using Wolverine.Attributes;

namespace PetHotel.Operations.Application.Incidents.ReportIncident;

/// <summary>
/// Registra o incidente vinculado à estadia (valida estadia via gateway, deriva o pet). Fora do
/// Wolverine (dois DbContexts: Operations grava + Booking lê) — invocado direto via DI no endpoint.
/// </summary>
[WolverineIgnore]
public static class ReportIncidentHandler
{
    public static async Task<Result<Guid>> Handle(
        ReportIncident command,
        IValidator<ReportIncident> validator,
        ITenantContext tenantContext,
        IStayGateway stays,
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var stay = await stays.FindStayAsync(command.ReservationId, cancellationToken);
        if (stay is null)
        {
            return Error.NotFound("reservation.not_found", "Reserva não encontrada.");
        }

        if (!stay.Arrived)
        {
            return Error.Conflict("stay.not_active", "O pet não está em estadia — registre o incidente após o check-in.");
        }

        var now = clock.GetUtcNow();
        var result = Incident.Report(
            tenantContext.Current, new PetReference(stay.PetId), CareContextType.HotelStay, command.ReservationId,
            command.Severity, command.Description, command.OccurredAt ?? now, now);

        if (result.IsFailure)
        {
            return result.Error;
        }

        incidents.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
