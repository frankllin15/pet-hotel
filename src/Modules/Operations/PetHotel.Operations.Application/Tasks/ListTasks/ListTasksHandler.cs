using PetHotel.Operations.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Tasks.ListTasks;

/// <summary>Projeta as tarefas do dia direto para DTO (docs/04).</summary>
public static class ListTasksHandler
{
    public static async Task<Result<IReadOnlyList<OperationalTaskDto>>> Handle(
        ListTasks query,
        IOperationalTaskQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.ListByDateAsync(query.Date, cancellationToken));
    }
}
