using FluentValidation;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Validation;
using PetHotel.Operations.Domain.Ports;
using PetHotel.Operations.Domain.Tasks;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Tasks.CreateTask;

/// <summary>Cria uma tarefa operacional no tenant corrente (contexto único → via bus).</summary>
public static class CreateTaskHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateTask command,
        IValidator<CreateTask> validator,
        ITenantContext tenantContext,
        IOperationalTaskRepository tasks,
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

        var result = OperationalTask.Create(
            tenantContext.Current, command.Title, command.Date, command.Category, command.AssignedTo);
        if (result.IsFailure)
        {
            return result.Error;
        }

        tasks.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id.Value;
    }
}
