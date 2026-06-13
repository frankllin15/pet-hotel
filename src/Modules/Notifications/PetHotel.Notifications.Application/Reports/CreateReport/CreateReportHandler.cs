using FluentValidation;
using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Application.Validation;
using PetHotel.Notifications.Domain.Ports;
using PetHotel.Notifications.Domain.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.Application.Reports.CreateReport;

/// <summary>Persiste o relatório (rascunho). O conteúdo é montado fora (a partir do diário, docs/03).</summary>
public static class CreateReportHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateReport command,
        IValidator<CreateReport> validator,
        ITenantContext tenantContext,
        IOutboundMessageRepository messages,
        IUnitOfWork unitOfWork,
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

        var result = OutboundMessage.CreateReport(
            tenantContext.Current,
            new TutorReference(command.TutorId),
            new PetReference(command.PetId),
            command.ReservationId,
            command.ReportDate,
            command.Title,
            command.Content);

        if (result.IsFailure)
        {
            return result.Error;
        }

        messages.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
