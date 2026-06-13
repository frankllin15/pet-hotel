using FluentValidation;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Validation;
using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Medications;
using PetHotel.Operations.Domain.Ports;
using PetHotel.SharedKernel;
using Wolverine.Attributes;

namespace PetHotel.Operations.Application.Medications.RecordMedication;

/// <summary>
/// Registra a medicação vinculada à estadia (valida estadia via gateway, deriva o pet). Fora do
/// Wolverine (dois DbContexts: Operations grava + Booking lê) — invocado direto via DI no endpoint.
/// </summary>
[WolverineIgnore]
public static class RecordMedicationHandler
{
    public static async Task<Result<Guid>> Handle(
        RecordMedication command,
        IValidator<RecordMedication> validator,
        ITenantContext tenantContext,
        IStayGateway stays,
        IMedicationRepository medications,
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
            return Error.Conflict("stay.not_active", "O pet não está em estadia — registre a medicação após o check-in.");
        }

        var now = clock.GetUtcNow();
        var result = MedicationAdministration.Administer(
            tenantContext.Current, new PetReference(stay.PetId), CareContextType.HotelStay, command.ReservationId,
            command.Drug, command.Dose, command.AdministeredAt ?? now, now);

        if (result.IsFailure)
        {
            return result.Error;
        }

        medications.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
