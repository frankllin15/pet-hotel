using FluentValidation;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Validation;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;
using Wolverine.Attributes;

namespace PetHotel.Operations.Application.CareLog.LogCareEntry;

/// <summary>
/// Registra a ocorrência vinculada à estadia: valida que a reserva existe e que o pet chegou
/// (check-in feito) via o gateway de estadia, deriva o pet da estadia e persiste (docs/03).
/// </summary>
/// <remarks>
/// Fora do Wolverine (<see cref="WolverineIgnoreAttribute"/>): toca dois DbContexts (Operations
/// na escrita + Booking no gateway de leitura), então a auto-transação do Wolverine não consegue
/// decidir o contexto. Invocado direto via DI no endpoint (mesmo padrão do ConfirmReservation).
/// </remarks>
[WolverineIgnore]
public static class LogCareEntryHandler
{
    public static async Task<Result<Guid>> Handle(
        LogCareEntry command,
        IValidator<LogCareEntry> validator,
        ITenantContext tenantContext,
        IStayGateway stays,
        ICareLogRepository entries,
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
            return Error.Conflict("stay.not_active", "O pet não está em estadia — registre o diário após o check-in.");
        }

        var now = clock.GetUtcNow();
        var result = CareLogEntry.Log(
            tenantContext.Current,
            new PetReference(stay.PetId),
            CareContextType.HotelStay,
            command.ReservationId,
            command.Type,
            command.Note,
            command.OccurredAt ?? now,
            now);

        if (result.IsFailure)
        {
            return result.Error;
        }

        entries.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
