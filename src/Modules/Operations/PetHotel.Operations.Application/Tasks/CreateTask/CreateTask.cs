using PetHotel.Operations.Domain.Tasks;

namespace PetHotel.Operations.Application.Tasks.CreateTask;

/// <summary>Cria uma tarefa operacional para um dia (com responsável opcional).</summary>
public sealed record CreateTask(string Title, DateOnly Date, TaskCategory Category, Guid? AssignedTo);
