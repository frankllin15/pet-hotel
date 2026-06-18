using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Domain.Ports;
using PetHotel.Operations.Domain.Tasks;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Tasks.SetTaskDone;

/// <summary>Alterna o estado feito/não-feito de uma tarefa (contexto único → via bus).</summary>
public static class SetTaskDoneHandler
{
    public static async Task<Result> Handle(
        SetTaskDone command,
        IOperationalTaskRepository tasks,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var task = await tasks.FindAsync(new OperationalTaskId(command.Id), cancellationToken);
        if (task is null)
        {
            return Error.NotFound("task.not_found", "Tarefa não encontrada.");
        }

        task.SetDone(command.Done, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
