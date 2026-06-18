using PetHotel.Operations.Domain.Tasks;

namespace PetHotel.Operations.Application.Tasks.UpdateTask;

/// <summary>Edita título, categoria e responsável de uma tarefa.</summary>
public sealed record UpdateTask(Guid Id, string Title, TaskCategory Category, Guid? AssignedTo);
