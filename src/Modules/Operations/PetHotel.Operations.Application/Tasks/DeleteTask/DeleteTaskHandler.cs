using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Domain.Ports;
using PetHotel.Operations.Domain.Tasks;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Tasks.DeleteTask;

/// <summary>Exclui uma tarefa do tenant corrente (contexto único → via bus).</summary>
public static class DeleteTaskHandler
{
    public static async Task<Result> Handle(
        DeleteTask command,
        IOperationalTaskRepository tasks,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var task = await tasks.FindAsync(new OperationalTaskId(command.Id), cancellationToken);
        if (task is null)
        {
            return Error.NotFound("task.not_found", "Tarefa não encontrada.");
        }

        tasks.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
