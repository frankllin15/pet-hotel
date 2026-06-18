namespace PetHotel.Operations.Application.Tasks.SetTaskDone;

/// <summary>Marca uma tarefa como feita/não-feita.</summary>
public sealed record SetTaskDone(Guid Id, bool Done);
