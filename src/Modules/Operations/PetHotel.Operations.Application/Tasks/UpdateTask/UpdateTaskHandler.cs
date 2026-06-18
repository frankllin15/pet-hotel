using FluentValidation;
using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Validation;
using PetHotel.Operations.Domain.Ports;
using PetHotel.Operations.Domain.Tasks;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Tasks.UpdateTask;

/// <summary>Carrega a tarefa do tenant corrente e aplica a edição (contexto único → via bus).</summary>
public static class UpdateTaskHandler
{
    public static async Task<Result> Handle(
        UpdateTask command,
        IValidator<UpdateTask> validator,
        IOperationalTaskRepository tasks,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var task = await tasks.FindAsync(new OperationalTaskId(command.Id), cancellationToken);
        if (task is null)
        {
            return Error.NotFound("task.not_found", "Tarefa não encontrada.");
        }

        var update = task.Update(command.Title, command.Category, command.AssignedTo);
        if (update.IsFailure)
        {
            return update.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
